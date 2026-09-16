using FEwS.Forums.Domain.UseCases.SignOn;
using FEwS.Forums.Storage.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace FEwS.Forums.Storage.Storages;

internal class SignOnStorage(
    ForumDbContext dbContext,
    IGuidFactory guidFactory) : ISignOnStorage
{
    public async Task<CreateUserResult> CreateUserAsync(string userName, string passwordHash, CancellationToken cancellationToken)
    {
        Guid userId = guidFactory.Create();
        var user = new User
        {
            Id = userId,
            UserName = userName,
            PasswordHash = passwordHash
        };
        await dbContext.Users.AddAsync(user, cancellationToken);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
            ConstraintName: ForumDbContext.UserNameIndexName
        })
        {
            dbContext.Entry(user).State = EntityState.Detached;
            return new CreateUserResult.DuplicateUserName();
        }

        return new CreateUserResult.Created(userId);
    }
}
