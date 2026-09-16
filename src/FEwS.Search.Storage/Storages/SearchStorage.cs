using OpenSearch.Client;
using FEwS.Search.Domain.Models;
using FEwS.Search.Domain.UseCases.Search;
using Entities_SearchEntity = FEwS.Search.Storage.Entities.SearchEntity;

namespace FEwS.Search.Storage.Storages;

internal class SearchStorage(IOpenSearchClient client) : ISearchStorage
{
    public async Task<(IEnumerable<SearchResult> resources, int totalCount)> Search(
        string query,
        IReadOnlyCollection<SearchEntityType> searchIn,
        int skip,
        int size,
        CancellationToken cancellationToken)
    {
        ISearchResponse<Entities_SearchEntity> searchResponse = await client.SearchAsync<Entities_SearchEntity>(descriptor => descriptor
            .From(skip)
            .Size(size)
            .Query(q => q
                .Bool(b => b
                    .Should(
                        s => s.Match(m => m
                            .Field(se => se.Title).Query(query)),
                        s => s.Match(m => m
                            .Field(se => se.Text).Query(query).Fuzziness(Fuzziness.EditDistance(1))))
                    .MinimumShouldMatch(1)
                    .Filter(searchIn.Count == 0
                        ? []
                        : [f => f.Terms(t => t
                            .Field(se => se.EntityType)
                            .Terms(searchIn.Select(entityType => (object)(int)entityType)))])))
            .Highlight(h => h
                .Fields(
                    f => f.Field(se => se.Title),
                    f => f.Field(se => se.Text).PreTags("<mark>").PostTags("</mark>"))),
            cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();
        if (!searchResponse.IsValid)
        {
            throw new InvalidOperationException(
                $"OpenSearch search failed. HTTP status: {searchResponse.ApiCall?.HttpStatusCode}.",
                searchResponse.OriginalException);
        }

        IEnumerable<SearchResult> searchResults = searchResponse.Hits.Select(hit =>
            new SearchResult
        {
            EntityId = hit.Source.EntityId,
            EntityType = (SearchEntityType)hit.Source.EntityType,
            Highlights = hit.Highlight.Values.SelectMany(v => v).ToArray()
        });
        return (searchResults.ToArray(), (int)searchResponse.Total);
    }
}
