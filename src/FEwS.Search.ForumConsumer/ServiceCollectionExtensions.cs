using FEwS.Search.API.Grpc;
using Microsoft.Extensions.Options;

namespace FEwS.Search.ForumConsumer;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAuthenticatedSearchClient(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<IndexingAuthenticationOptions>()
            .Bind(configuration.GetSection("Indexing"))
            .Validate(options => options.ApiKey.Length == 64 && options.ApiKey.All(char.IsAsciiHexDigit),
                "Indexing:ApiKey must contain 64 hexadecimal characters")
            .ValidateOnStart();
        var searchEngineAddress = new Uri(configuration.GetConnectionString("SearchEngine") ?? throw new InvalidOperationException());
        if (searchEngineAddress.Scheme != Uri.UriSchemeHttps)
        {
            throw new InvalidOperationException("SearchEngine must use HTTPS for indexing authentication");
        }
        services.AddGrpcClient<SearchEngine.SearchEngineClient>(options => options.Address = searchEngineAddress)
            .AddCallCredentials((_, metadata, serviceProvider) =>
            {
                string apiKey = serviceProvider.GetRequiredService<IOptions<IndexingAuthenticationOptions>>().Value.ApiKey;
                metadata.Add("x-indexing-key", apiKey);
                return Task.CompletedTask;
            });
        return services;
    }
}
