using Confluent.Kafka;
using FEwS.Search.ForumConsumer;
using FEwS.Search.ForumConsumer.Monitoring;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApiLogging(builder.Configuration, builder.Environment)
    .AddApiMetrics(builder.Configuration, builder.Environment);

builder.Services.AddAuthenticatedSearchClient(builder.Configuration);

builder.Services.Configure<ConsumerConfig>(builder.Configuration.GetSection("Kafka").Bind);
builder.Services.PostConfigure<ConsumerConfig>(options =>
{
    options.EnableAutoCommit = false;
    options.EnableAutoOffsetStore = false;
    options.MaxPollIntervalMs = Math.Max(options.MaxPollIntervalMs ?? 300000, 120000);
});
builder.Services.Configure<ProducerConfig>(builder.Configuration.GetSection("Kafka").Bind);
builder.Services.PostConfigure<ProducerConfig>(options =>
{
    options.EnableIdempotence = true;
    options.Acks = Acks.All;
    options.MessageTimeoutMs = 10000;
});
builder.Services.AddSingleton<KafkaClientFactory>();

builder.Services.AddHostedService<ForumSearchConsumer>();

WebApplication app = builder.Build();

app.Run();
