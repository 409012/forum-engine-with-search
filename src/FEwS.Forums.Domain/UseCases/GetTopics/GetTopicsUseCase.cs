using FEwS.Forums.Domain.Models;
using FEwS.Forums.Domain.UseCases.GetForums;
using MediatR;

namespace FEwS.Forums.Domain.UseCases.GetTopics;

internal class GetTopicsUseCase(
    IGetForumsStorage getForumsStorage,
    IGetTopicsStorage storage)
    : IRequestHandler<GetTopicsQuery, (IEnumerable<Topic> resources, int totalCount)>
{
    public async Task<(IEnumerable<Topic> resources, int totalCount)> Handle(
        GetTopicsQuery query, CancellationToken cancellationToken)
    {
        await getForumsStorage.ThrowIfForumNotFound(query.ForumId, cancellationToken);
        return await storage.GetTopicsAsync(query.ForumId, query.Skip, query.Take, cancellationToken);
    }
}