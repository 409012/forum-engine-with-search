namespace FEwS.Forums.Storage.Models;

public class ForumDomainEvent
{
    public ForumDomainEventType EventType { get; set; }

    public Guid TopicId { get; set; }

    public required string Title { get; set; }
    
    public ForumComment? Comment { get; set; }

    public class ForumComment
    {
        public Guid CommentId { get; set; }
        public required string Text { get; set; }
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