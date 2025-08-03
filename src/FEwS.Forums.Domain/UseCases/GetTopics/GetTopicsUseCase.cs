using FEwS.Forums.Domain.Models;
using FEwS.Forums.Domain.UseCases.GetForums;
using MediatR;

namespace FEwS.Forums.Domain.UseCases.GetTopics;

internal class GetTopicsUseCase(
    IGetForumsStorage getForumsStorage,
    IGetTopicsStorage storage)
    : IRequestHandler<GetTopicsQuery, TopicsPagedResult>
{
    public async Task<TopicsPagedResult> Handle(
        GetTopicsQuery query, CancellationToken cancellationToken)
    {
        await getForumsStorage.ThrowIfForumNotFound(query.ForumId, cancellationToken);
        return await storage.GetTopicsAsync(query.ForumId, query.Skip, query.Take, cancellationToken);
    }
}