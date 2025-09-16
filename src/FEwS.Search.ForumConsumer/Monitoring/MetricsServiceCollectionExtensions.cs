using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace FEwS.Search.ForumConsumer.Monitoring;

internal static class MetricsServiceCollectionExtensions
{
    public static IServiceCollection AddApiMetrics(this IServiceCollection services, IConfiguration configuration,
        IWebHostEnvironment builderEnvironment)
    {
        services
            .AddOpenTelemetry()
            .WithMetrics(builder => builder
                .AddAspNetCoreInstrumentation()
                .AddPrometheusExporter())
            .WithTracing(builder => builder
                .ConfigureResource(r => r.AddService("FEwS.Search.ForumConsumer"))
                .AddAspNetCoreInstrumentation(options =>
                {
                    options.Filter += context =>
                        context.Request.Path.Value != null &&
                        !context.Request.Path.Value.Contains("metrics", StringComparison.InvariantCultureIgnoreCase) &&
                        !context.Request.Path.Value.Contains("swagger", StringComparison.InvariantCultureIgnoreCase);
                    options.EnrichWithHttpResponse = (activity, response) =>
                        activity.AddTag("error", response.StatusCode >= 400);
                })
                .AddSource(ForumConsumerMetrics.ApplicationName)
                .AddGrpcClientInstrumentation()
                .AddHttpClientInstrumentation()
                .AddJaegerExporter(cfg => cfg.Endpoint = new Uri(configuration.GetConnectionString("Tracing") ?? throw new InvalidOperationException())));

        return services;
    }
}