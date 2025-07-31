using FEwS.Search.Domain.Models;
using MediatR;

namespace FEwS.Search.Domain.UseCases.Search;

public record SearchQuery(string Query) : IRequest<(IEnumerable<SearchResult> resources, int totalCount)>;