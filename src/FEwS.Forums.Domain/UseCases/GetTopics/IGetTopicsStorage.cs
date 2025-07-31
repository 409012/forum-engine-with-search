using FEwS.Forums.Domain.Models;

namespace FEwS.Forums.Domain.UseCases.GetTopics;

public interface IGetTopicsStorage
{
    Task<(IEnumerable<Topic> resources, int totalCount)> GetTopicsAsync(
        Guid forumId, int skip, int take, CancellationToken cancellationToken);
}