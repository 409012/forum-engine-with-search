using FEwS.Forums.Domain.UseCases.SignOut;
using Microsoft.EntityFrameworkCore;

namespace FEwS.Forums.Storage.Storages;

internal class SignOutStorage(ForumDbContext dbContext) : ISignOutStorage
{
    public Task RemoveSessionAsync(Guid sessionId, CancellationToken cancellationToken)
    {
        return dbContext.Sessions
            .Where(session => session.SessionId == sessionId)
            .ExecuteDeleteAsync(cancellationToken);
    }
}
