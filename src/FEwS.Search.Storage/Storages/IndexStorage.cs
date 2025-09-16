using OpenSearch.Client;
using FEwS.Search.Domain.Models;
using FEwS.Search.Domain.UseCases.Index;
using Entities_SearchEntity = FEwS.Search.Storage.Entities.SearchEntity;

namespace FEwS.Search.Storage.Storages;

internal class IndexStorage(IOpenSearchClient client) : IIndexStorage
{
    public async Task Index(Guid entityId, SearchEntityType entityType, string? title, string? text,
        CancellationToken cancellationToken)
    {
        await client.IndexAsync(new Entities_SearchEntity
        {
            EntityId = entityId,
            EntityType = (int)entityType,
            Title = title,
            Text = text,
        }, descriptor => descriptor, cancellationToken);
    }
}