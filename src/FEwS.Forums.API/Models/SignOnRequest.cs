namespace FEwS.Forums.API.Models;

public class SignOnRequest
{
    public required string UserName { get; set; }
    public required string Password { get; set; }
}