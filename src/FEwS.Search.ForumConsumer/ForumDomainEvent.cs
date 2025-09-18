using JetBrains.Annotations;

namespace FEwS.Search.ForumConsumer;

public class DomainEventWrapper
{
    public required string ContentBlob { get; set; }
}

public class ForumDomainEvent
{
    public ForumDomainEventType EventType { get; set; }

    public Guid TopicId { get; set; }

    public string? Title { get; set; }
    
    public ForumComment? Comment { get; set; }

    public class ForumComment
    {
        [UsedImplicitly]
        public Guid CommentId { get; set; }
        
        [UsedImplicitly]
        public string? Text { get; set; }
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