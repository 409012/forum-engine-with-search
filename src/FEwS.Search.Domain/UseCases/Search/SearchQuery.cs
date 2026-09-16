using FEwS.Search.Domain.Models;
using MediatR;

namespace FEwS.Search.Domain.UseCases.Search;

public record SearchQuery(
    string Query,
    IReadOnlyCollection<SearchEntityType> SearchIn,
    int Skip,
    int Size) : IRequest<(IEnumerable<SearchResult> resources, int totalCount)>
{
    public const int DefaultSize = 10;
    public const int MaximumSize = 100;
}
