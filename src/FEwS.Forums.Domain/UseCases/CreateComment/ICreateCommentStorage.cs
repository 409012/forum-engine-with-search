using FEwS.Forums.Domain.Models;

namespace FEwS.Forums.Domain.UseCases.CreateComment;

public interface ICreateCommentStorage : IStorage
{
    Task<Topic?> FindTopicAsync(Guid topicId, CancellationToken cancellationToken);
    Task<Comment> CreateCommentAsync(Guid topicId, Guid userId, string text, CancellationToken cancellationToken);
}