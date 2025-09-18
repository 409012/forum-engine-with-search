using JetBrains.Annotations;

namespace FEwS.Forums.API.Models;

public class SignInRequest
{
    [UsedImplicitly]
    public required string UserName { get; set; }
    
    [UsedImplicitly]
    public required string Password { get; set; }
}