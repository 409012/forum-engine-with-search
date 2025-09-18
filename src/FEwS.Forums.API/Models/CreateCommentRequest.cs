using JetBrains.Annotations;

namespace FEwS.Forums.API.Models;

public class CreateCommentRequest
{
    [UsedImplicitly]
    public required string Text { get; set; }
}