namespace FEwS.Forums.Domain.Models;

public class TopicReadModel
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public required string Title { get; set; }
    public int TotalCommentsCount { get; set; }
    public TopicTopicReadModelComment? LastComment { get; set; }
}

public class TopicTopicReadModelComment
{
    public Guid? Id { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
}