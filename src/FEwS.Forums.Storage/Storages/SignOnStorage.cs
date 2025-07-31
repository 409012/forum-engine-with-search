using FEwS.Forums.Domain.UseCases.SignOn;
using FEwS.Forums.Storage.Entities;

namespace FEwS.Forums.Storage.Storages;

internal class SignOnStorage(
    ForumDbContext dbContext,
    IGuidFactory guidFactory) : ISignOnStorage
{
    public async Task<Guid> CreateUserAsync(string login, byte[] salt, byte[] hash, CancellationToken cancellationToken)
    {
        var userId = guidFactory.Create();
        await dbContext.Users.AddAsync(new User
        {
            UserId = userId,
            Login = login,
            Salt = salt,
            PasswordHash = hash,
        }, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return userId;
    }
}