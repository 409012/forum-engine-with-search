using FEwS.Search.Domain.Models;

namespace FEwS.Search.Domain.UseCases.Search;

public interface ISearchStorage
{
    Task<(IEnumerable<SearchResult> resources, int totalCount)> Search(
        string query,
        IReadOnlyCollection<SearchEntityType> searchIn,
        int skip,
        int size,
        CancellationToken cancellationToken);
}
