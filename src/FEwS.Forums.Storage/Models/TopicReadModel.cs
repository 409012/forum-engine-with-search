using JetBrains.Annotations;

namespace FEwS.Forums.Storage.Models;

internal class TopicReadModel
{
    [UsedImplicitly]
    public Guid TopicId { get; set; }
    
    public Guid ForumId { get; set; }
    
    public Guid UserId { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    
    public required string Title { get; set; }
    
    public int TotalCommentsCount { get; set; }
    
    [UsedImplicitly]
    public DateTimeOffset? LastCommentCreatedAt { get; set; }
    
    [UsedImplicitly]
    public Guid? LastCommentId { get; set; }
}