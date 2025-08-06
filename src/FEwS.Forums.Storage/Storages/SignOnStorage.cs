using FEwS.Forums.Domain.UseCases.SignOn;
using FEwS.Forums.Storage.Entities;
using Microsoft.AspNetCore.Identity;

namespace FEwS.Forums.Storage.Storages;

internal class SignOnStorage(
    ForumDbContext dbContext,
    IGuidFactory guidFactory) : ISignOnStorage
{
    public async Task<Guid> CreateUserAsync(string userName, string passwordHash, CancellationToken cancellationToken)
    {
        var userId = guidFactory.Create();
        await dbContext.Users.AddAsync(new User
        {
            Id = userId,
            UserName = userName,
            PasswordHash = passwordHash
        }, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return userId;
    }
}