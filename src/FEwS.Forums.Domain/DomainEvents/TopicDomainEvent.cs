using FEwS.Forums.Domain.Models;

namespace FEwS.Forums.Domain.DomainEvents;

public class ForumDomainEvent
{
    private ForumDomainEvent()
    {
    }

    public ForumDomainEventType EventType { get; init; }

    public Guid TopicId { get; init; }

    public string? Title { get; init; }
    
    public ForumComment? Comment { get; init; }

    public class ForumComment
    {
        public Guid CommentId { get; init; }
        public string? Text { get; init; }
    }

    public static ForumDomainEvent TopicCreated(Topic topic)
    {
        return new ForumDomainEvent
        {
            EventType = ForumDomainEventType.TopicCreated,
            TopicId = topic.Id,
            Title = topic.Title
        };
    }

    public static ForumDomainEvent CommentCreated(Topic topic, Comment comment)
    {
        return new ForumDomainEvent
        {
            EventType = ForumDomainEventType.CommentCreated,
            TopicId = topic.Id,
            Title = topic.Title,
            Comment = new ForumComment
            {
                CommentId = comment.Id,
                Text = comment.Text
            }
        };
    }
}

public enum ForumDomainEventType
{
    TopicCreated = 100,
    TopicUpdated = 101,
    TopicDeleted = 102,

    CommentCreated = 200,
    CommentUpdated = 201,
    CommentDeleted = 202,
}