using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using FEwS.Forums.Domain.UseCases.SignIn;
using FEwS.Forums.Storage.Entities;
using User = FEwS.Forums.Domain.Models.User;

namespace FEwS.Forums.Storage.Storages;

internal class SignInStorage(
    IGuidFactory guidFactory,
    ForumDbContext dbContext,
    IMapper mapper)
    : ISignInStorage
{
    public Task<User?> FindUserAsync(string userName, CancellationToken cancellationToken)
    {
        return dbContext.Users
            .Where(u => u.UserName != null && u.UserName.Equals(userName))
            .ProjectTo<User>(mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Guid> CreateSessionAsync(
        Guid userId, DateTimeOffset expirationMoment, CancellationToken cancellationToken)
    {
        Guid sessionId = guidFactory.Create();
        await dbContext.Sessions.AddAsync(new Session
        {
            SessionId = sessionId,
            UserId = userId,
            ExpiresAt = expirationMoment,
        }, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return sessionId;
    }
}