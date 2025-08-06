using System.Security.Principal;
using Microsoft.AspNetCore.Identity;
namespace FEwS.Forums.Domain.Models;

public class User
{
    public User()
    {
        UserId = Guid.Empty;
        PasswordHash = string.Empty;
    }
    public Guid UserId { get; set; }
    
    public string PasswordHash { get; set; }
}