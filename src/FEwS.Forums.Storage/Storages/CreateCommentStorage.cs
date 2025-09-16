using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using FEwS.Forums.Domain.Models;
using FEwS.Forums.Domain.UseCases.CreateComment;

namespace FEwS.Forums.Storage.Storages;

internal class CreateCommentStorage(
    ForumDbContext dbContext,
    IMapper mapper,
    IGuidFactory guidFactory,
    TimeProvider timeProvider) : ICreateCommentStorage
{
    public Task<Topic?> FindTopicAsync(Guid topicId, CancellationToken cancellationToken)
    {
        return dbContext.Topics
            .Where(t => t.TopicId == topicId)
            .ProjectTo<Topic>(mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Comment> CreateCommentAsync(Guid topicId, Guid userId, string text, CancellationToken cancellationToken)
    {
        Guid commentId = guidFactory.Create();
        await dbContext.Comments.AddAsync(new Entities.Comment
        {
            CommentId = commentId,
            TopicId = topicId,
            UserId = userId,
            CreatedAt = timeProvider.GetUtcNow(),
            Text = text
        }, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return await dbContext.Comments
            .Where(c => c.CommentId == commentId)
            .ProjectTo<Comment>(mapper.ConfigurationProvider)
            .FirstAsync(cancellationToken);
    }
}