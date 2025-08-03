using FEwS.Forums.Domain.Models;

namespace FEwS.Forums.Domain.UseCases.GetTopics;

public interface IGetTopicsStorage
{
    Task<TopicsPagedResult> GetTopicsAsync(
        Guid forumId, int skip, int take, CancellationToken cancellationToken);
}