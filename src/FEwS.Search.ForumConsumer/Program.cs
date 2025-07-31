using Confluent.Kafka;
using Microsoft.Extensions.Options;
using FEwS.Search.API.Grpc;
using FEwS.Search.ForumConsumer;
using FEwS.Search.ForumConsumer.Monitoring;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApiLogging(builder.Configuration, builder.Environment)
    .AddApiMetrics(builder.Configuration, builder.Environment);

builder.Services.AddGrpcClient<SearchEngine.SearchEngineClient>(options =>
        options.Address = new Uri(builder.Configuration.GetConnectionString("SearchEngine")!))
    .ConfigurePrimaryHttpMessageHandler(() =>
        new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        });

builder.Services.Configure<ConsumerConfig>(builder.Configuration.GetSection("Kafka").Bind);
builder.Services.AddSingleton(sp => new ConsumerBuilder<byte[], byte[]>(
    sp.GetRequiredService<IOptions<ConsumerConfig>>().Value).Build());

builder.Services.AddHostedService<ForumSearchConsumer>();

var app = builder.Build();

app.Run();