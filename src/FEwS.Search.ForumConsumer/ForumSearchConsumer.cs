using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using FEwS.Search.API.Grpc;
using FEwS.Search.ForumConsumer.Monitoring;

namespace FEwS.Search.ForumConsumer;

internal class ForumSearchConsumer(
    IConsumer<byte[], byte[]> consumer,
    SearchEngine.SearchEngineClient searchEngineClient,
    IOptions<ConsumerConfig> consumerConfig) : BackgroundService
{
    private readonly ConsumerConfig consumerConfig = consumerConfig.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();

        consumer.Subscribe("fews.DomainEvents");

        while (!stoppingToken.IsCancellationRequested)
        {
            ConsumeResult<byte[], byte[]> consumeResult = consumer.Consume(stoppingToken);
            if (consumeResult is not { IsPartitionEOF: false })
            {
                await Task.Delay(300, stoppingToken);
                continue;
            }

            string? activityId = consumeResult.Message.Headers.TryGetLastBytes("activity_id", out byte[]? lastBytes)
                ? Encoding.UTF8.GetString(lastBytes)
                : null;
        
            using Activity? activity = ForumConsumerMetrics.ActivitySource.StartActivity("consumer", ActivityKind.Consumer,
                ActivityContext.TryParse(activityId, null, out ActivityContext context) ? context : default);
            activity?.AddTag("messaging.system", "kafka");
            activity?.AddTag("messaging.destination.name", "fews.DomainEvents");
            activity?.AddTag("messaging.kafka.consumer_group", consumerConfig.GroupId);
            activity?.AddTag("messaging.kafka.partition", consumeResult.Partition);

            DomainEventWrapper domainEventWrapper = JsonSerializer.Deserialize<DomainEventWrapper>(consumeResult.Message.Value) ?? throw new InvalidOperationException();
            byte[] contentBlob = Convert.FromBase64String(domainEventWrapper.ContentBlob);
            ForumDomainEvent domainEvent = JsonSerializer.Deserialize<ForumDomainEvent>(contentBlob) ?? throw new InvalidOperationException();
        
            switch (domainEvent.EventType)
            {
                case ForumDomainEventType.TopicCreated:
                    await searchEngineClient.IndexAsync(new IndexRequest
                    {
                        Id = domainEvent.TopicId.ToString(),
                        Type = SearchEntityType.ForumTopic,
                        Title = domainEvent.Title
                    }, cancellationToken: stoppingToken);
                    break;
                case ForumDomainEventType.CommentCreated:
                    if (domainEvent.Comment != null)
                    {
                        await searchEngineClient.IndexAsync(new IndexRequest
                            {
                                Id = domainEvent.Comment.CommentId.ToString(),
                                Type = SearchEntityType.ForumComment,
                                Text = domainEvent.Comment.Text
                            },
                            cancellationToken: stoppingToken);
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException(domainEvent.EventType.ToString());
            }
            consumer.Commit(consumeResult);
        }
        consumer.Close();
    }
}