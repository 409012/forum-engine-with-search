namespace FEwS.Search.API.Authentication;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIndexingAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<IndexingAuthenticationOptions>(IndexingAuthenticationOptions.SchemeName)
            .Bind(configuration.GetSection("Indexing"))
            .Validate(options => options.ApiKey.Length == 64 && options.ApiKey.All(char.IsAsciiHexDigit),
                "Indexing:ApiKey must contain 64 hexadecimal characters")
            .ValidateOnStart();
        services.AddAuthentication(IndexingAuthenticationOptions.SchemeName)
            .AddScheme<IndexingAuthenticationOptions, IndexingAuthenticationHandler>(IndexingAuthenticationOptions.SchemeName, _ => { });
        services.AddAuthorization(options => options.AddPolicy(IndexingAuthenticationOptions.SchemeName,
            policy => policy.AddAuthenticationSchemes(IndexingAuthenticationOptions.SchemeName).RequireAuthenticatedUser()));
        return services;
    }
}
