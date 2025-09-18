using JetBrains.Annotations;

namespace FEwS.Search.Domain.Models;

public class SearchEntity
{
    [UsedImplicitly]
    public Guid EntityId { get; set; }
    
    [UsedImplicitly]
    public SearchEntityType EntityType { get; set; }
    
    [UsedImplicitly]
    public string? Title { get; set; }
    
    [UsedImplicitly]
    public string? Text { get; set; }
}

public enum SearchEntityType
{
    ForumTopic,
    ForumComment
}