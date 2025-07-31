using FEwS.Search.Domain.Models;

namespace FEwS.Search.Domain.UseCases.Index;

public interface IIndexStorage
{
    Task Index(Guid entityId, SearchEntityType entityType, string? title, string? text,
        CancellationToken cancellationToken);
}