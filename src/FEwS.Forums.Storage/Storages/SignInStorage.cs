using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using FEwS.Forums.Domain.UseCases.SignIn;
using FEwS.Forums.Storage.Entities;

namespace FEwS.Forums.Storage.Storages;

internal class SignInStorage(
    IGuidFactory guidFactory,
    ForumDbContext dbContext,
    IMapper mapper)
    : ISignInStorage
{
    public Task<RecognisedUser?> FindUserAsync(string login, CancellationToken cancellationToken) => dbContext.Users
        .Where(u => u.Login.Equals(login))
        .ProjectTo<RecognisedUser>(mapper.ConfigurationProvider)
        .FirstOrDefaultAsync(cancellationToken);

    public async Task<Guid> CreateSessionAsync(
        Guid userId, DateTimeOffset expirationMoment, CancellationToken cancellationToken)
    {
        var sessionId = guidFactory.Create();
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