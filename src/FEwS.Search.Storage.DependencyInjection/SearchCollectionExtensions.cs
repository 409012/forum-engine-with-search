using Microsoft.Extensions.DependencyInjection;
using OpenSearch.Client;
using FEwS.Search.Domain.UseCases.Index;
using FEwS.Search.Domain.UseCases.Search;
using FEwS.Search.Storage.Entities;
using FEwS.Search.Storage.Storages;

namespace FEwS.Search.Storage.DependencyInjection;

public static class SearchCollectionExtensions
{
    public static IServiceCollection AddSearchStorage(this IServiceCollection services,
        string connectionString)
    {
        services
            .AddScoped<IIndexStorage, IndexStorage>()
            .AddScoped<ISearchStorage, SearchStorage>();

        services.AddSingleton<IOpenSearchClient>(new OpenSearchClient(new Uri(connectionString))
        {
            ConnectionSettings =
            {
                DefaultIndices = { [typeof(SearchEntity)] = "fews-search-v1" }
            }
        });

        return services;
    }
}