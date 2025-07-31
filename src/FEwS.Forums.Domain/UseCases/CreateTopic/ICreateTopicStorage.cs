using FEwS.Forums.Domain.Models;

namespace FEwS.Forums.Domain.UseCases.CreateTopic;

public interface ICreateTopicStorage : IStorage
{
    Task<Topic> CreateTopicAsync(Guid forumId, Guid userId, string title, CancellationToken cancellationToken);
}