using MediatR;

namespace FEwS.Search.Domain.UseCases.Index;

internal class IndexUseCase(IIndexStorage storage) : IRequestHandler<IndexCommand>
{
    public Task Handle(IndexCommand request, CancellationToken cancellationToken)
    {
        (Guid entityId, Models.SearchEntityType searchEntityType, string? title, string? text) = request;
        return storage.Index(entityId, searchEntityType, title, text, cancellationToken);
    }
}