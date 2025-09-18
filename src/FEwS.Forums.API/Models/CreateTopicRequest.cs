using JetBrains.Annotations;

namespace FEwS.Forums.API.Models;

public class CreateTopicRequest
{
    [UsedImplicitly]
    public required string Title { get; set; }
}