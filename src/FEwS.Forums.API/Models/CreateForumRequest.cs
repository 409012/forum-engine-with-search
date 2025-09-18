using JetBrains.Annotations;

namespace FEwS.Forums.API.Models;

public class CreateForumRequest
{
    [UsedImplicitly]
    public required string Title { get; set; }
}