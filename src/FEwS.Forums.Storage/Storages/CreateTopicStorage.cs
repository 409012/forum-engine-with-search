using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using FEwS.Forums.Domain.Models;
using FEwS.Forums.Domain.UseCases.CreateTopic;

namespace FEwS.Forums.Storage.Storages;

internal class CreateTopicStorage(
    IGuidFactory guidFactory,
    IMapper mapper,
    TimeProvider timeProvider,
    ForumDbContext dbContext)
    : ICreateTopicStorage
{
    public async Task<Topic> CreateTopicAsync(Guid forumId, Guid userId, string title,
        CancellationToken cancellationToken)
    {
        var topicId = guidFactory.Create();
        var topic = new Entities.Topic
        {
            TopicId = topicId,
            ForumId = forumId,
            UserId = userId,
            Title = title,
            CreatedAt = timeProvider.GetUtcNow(),
        };

        await dbContext.Topics.AddAsync(topic, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return await dbContext.Topics
            .Where(t => t.TopicId == topicId)
            .ProjectTo<Topic>(mapper.ConfigurationProvider)
            .FirstAsync(cancellationToken);
    }
}