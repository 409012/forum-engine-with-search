using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace FEwS.Forums.Storage.Entities;

public class User : IdentityUser<Guid>
{
    [InverseProperty(nameof(Topic.Author))]
    public ICollection<Topic>? Topics { get; init; }

    [InverseProperty(nameof(Comment.Author))]
    public ICollection<Comment>? Comments { get; init; }

    [InverseProperty(nameof(Session.User))]
    public ICollection<Session>? Sessions { get; init; }
}