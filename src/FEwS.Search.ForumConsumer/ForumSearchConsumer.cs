using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Confluent.Kafka;
using Grpc.Core;
using Microsoft.Extensions.Options;
using FEwS.Search.API.Grpc;
using FEwS.Search.ForumConsumer.Monitoring;

namespace FEwS.Search.ForumConsumer;

internal class ForumSearchConsumer(
    KafkaClientFactory kafkaClientFactory,
    SearchEngine.SearchEngineClient searchEngineClient,
    IOptions<ConsumerConfig> consumerConfig,
    ILogger<ForumSearchConsumer> logger) : BackgroundService
{
    private const string SourceTopic = "fews.DomainEvents";
    private const string DeadLetterTopic = "fews.DomainEvents.dead-letter";
    private const int MaximumIndexAttempts = 3;
    private readonly ConsumerConfig consumerConfig = consumerConfig.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ConsumeSessionAsync(stoppingToken);
                }
                catch (KafkaException exception)
                {
                    logger.LogError(exception, "Kafka session failed; reconnecting from committed offsets");
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
    }

    private async Task ConsumeSessionAsync(CancellationToken cancellationToken)
    {
        using IConsumer<byte[], byte[]> consumer = kafkaClientFactory.CreateConsumer();
        using IProducer<byte[], byte[]> producer = kafkaClientFactory.CreateProducer();

        try
        {
            consumer.Subscribe(SourceTopic);

            while (!cancellationToken.IsCancellationRequested)
            {
                ConsumeResult<byte[], byte[]> result = consumer.Consume(cancellationToken);
                if (result.IsPartitionEOF)
                {
                    continue;
                }

                await ProcessMessageAsync(producer, result, cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
                consumer.Commit(result);
            }
        }
        finally
        {
            try
            {
                consumer.Close();
            }
            catch (KafkaException exception)
            {
                logger.LogWarning(exception, "Could not close Kafka consumer cleanly");
            }
        }
    }

    private async Task ProcessMessageAsync(
        IProducer<byte[], byte[]> producer,
        ConsumeResult<byte[], byte[]> result,
        CancellationToken cancellationToken)
    {
        string? activityId = result.Message.Headers is { } headers
            && headers.TryGetLastBytes("activity_id", out byte[]? activityBytes)
            && activityBytes is not null
                ? Encoding.UTF8.GetString(activityBytes)
                : null;

        using Activity? activity = ForumConsumerMetrics.ActivitySource.StartActivity("consumer", ActivityKind.Consumer,
            ActivityContext.TryParse(activityId, null, out ActivityContext context) ? context : default);
        activity?.AddTag("messaging.system", "kafka");
        activity?.AddTag("messaging.destination.name", SourceTopic);
        activity?.AddTag("messaging.kafka.consumer_group", consumerConfig.GroupId);
        activity?.AddTag("messaging.kafka.partition", result.Partition);

        IndexRequest? request = CreateIndexRequest(result.Message.Value, out string failureReason);
        if (request is not null)
        {
            for (int attempt = 1; attempt <= MaximumIndexAttempts; attempt++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    await searchEngineClient.IndexAsync(request,
                        deadline: DateTime.UtcNow.AddSeconds(10), cancellationToken: cancellationToken);
                    return;
                }
                catch (RpcException exception)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    failureReason = $"Indexing failed with gRPC status {exception.StatusCode}";
                    logger.LogWarning("Indexing {Position} failed with {StatusCode} on attempt {Attempt}",
                        result.TopicPartitionOffset, exception.StatusCode, attempt);

                    if (!IsTransient(exception.StatusCode) || attempt == MaximumIndexAttempts)
                    {
                        break;
                    }

                    await Task.Delay(TimeSpan.FromSeconds(1 << (attempt - 1)), cancellationToken);
                }
            }
        }

        activity?.SetStatus(ActivityStatusCode.Error, failureReason);
        await PublishDeadLetterAsync(producer, result, failureReason, cancellationToken);
    }

    private static bool IsTransient(StatusCode statusCode)
    {
        return statusCode is StatusCode.Unavailable or StatusCode.DeadlineExceeded
            or StatusCode.ResourceExhausted or StatusCode.Aborted or StatusCode.Internal
            or StatusCode.Unknown or StatusCode.Cancelled;
    }

    private static IndexRequest? CreateIndexRequest(byte[]? value, out string failureReason)
    {
        failureReason = "Invalid event payload";
        if (value is null)
        {
            return null;
        }

        ForumDomainEvent? domainEvent;
        try
        {
            DomainEventWrapper? wrapper = JsonSerializer.Deserialize<DomainEventWrapper>(value);
            if (string.IsNullOrWhiteSpace(wrapper?.ContentBlob))
            {
                return null;
            }

            domainEvent = JsonSerializer.Deserialize<ForumDomainEvent>(Convert.FromBase64String(wrapper.ContentBlob));
        }
        catch (Exception exception) when (exception is JsonException or FormatException)
        {
            failureReason = $"Invalid event encoding: {exception.GetType().Name}";
            return null;
        }

        if (domainEvent is null || domainEvent.TopicId == Guid.Empty)
        {
            return null;
        }

        switch (domainEvent.EventType)
        {
            case ForumDomainEventType.TopicCreated when !string.IsNullOrWhiteSpace(domainEvent.Title):
                return new IndexRequest
                {
                    Id = domainEvent.TopicId.ToString(),
                    Type = SearchEntityType.ForumTopic,
                    Title = domainEvent.Title
                };
            case ForumDomainEventType.CommentCreated when domainEvent.Comment is { } comment
                && comment.CommentId != Guid.Empty && !string.IsNullOrWhiteSpace(comment.Text):
                return new IndexRequest
                {
                    Id = comment.CommentId.ToString(),
                    Type = SearchEntityType.ForumComment,
                    Text = comment.Text
                };
            default:
                failureReason = $"Unsupported event type or invalid fields: {domainEvent.EventType}";
                return null;
        }
    }

    private async Task PublishDeadLetterAsync(
        IProducer<byte[], byte[]> producer,
        ConsumeResult<byte[], byte[]> result,
        string reason,
        CancellationToken cancellationToken)
    {
        var headers = new Headers();
        if (result.Message.Headers is { } originalHeaders)
        {
            foreach (IHeader header in originalHeaders)
            {
                headers.Add(header.Key, header.GetValueBytes());
            }
        }

        var deadLetter = new DeadLetterEvent(result.Topic, result.Partition.Value, result.Offset.Value,
            consumerConfig.GroupId, reason, DateTimeOffset.UtcNow, result.Message.Key, result.Message.Value);
        DeliveryResult<byte[], byte[]> delivery = await producer.ProduceAsync(DeadLetterTopic,
            new Message<byte[], byte[]>
            {
                Key = Encoding.UTF8.GetBytes($"{result.Topic}:{result.Partition.Value}:{result.Offset.Value}"),
                Value = JsonSerializer.SerializeToUtf8Bytes(deadLetter),
                Headers = headers
            }, cancellationToken);

        if (delivery.Status != PersistenceStatus.Persisted)
        {
            throw new KafkaException(new Error(ErrorCode.Local_MsgTimedOut, "Dead-letter persistence was not confirmed"));
        }

        logger.LogError("Event {Position} saved to {DeadLetterTopic}: {Reason}",
            result.TopicPartitionOffset, DeadLetterTopic, reason);
    }

    private sealed record DeadLetterEvent(
        string SourceTopic,
        int SourcePartition,
        long SourceOffset,
        string? ConsumerGroup,
        string Reason,
        DateTimeOffset FailedAt,
        byte[]? OriginalKey,
        byte[]? OriginalValue);
}
