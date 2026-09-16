using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace FEwS.Search.ForumConsumer;

internal sealed class KafkaClientFactory(
    IOptions<ConsumerConfig> consumerConfig,
    IOptions<ProducerConfig> producerConfig)
{
    public IConsumer<byte[], byte[]> CreateConsumer()
    {
        return new ConsumerBuilder<byte[], byte[]>(consumerConfig.Value).Build();
    }

    public IProducer<byte[], byte[]> CreateProducer()
    {
        return new ProducerBuilder<byte[], byte[]>(producerConfig.Value).Build();
    }
}
