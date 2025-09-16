using FEwS.Forums.Domain.UseCases.SignOn;
using FEwS.Forums.Storage.Entities;

namespace FEwS.Forums.Storage.Storages;

internal class SignOnStorage(
    ForumDbContext dbContext,
    IGuidFactory guidFactory) : ISignOnStorage
{
    public async Task<Guid> CreateUserAsync(string userName, string passwordHash, CancellationToken cancellationToken)
    {
        Guid userId = guidFactory.Create();
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