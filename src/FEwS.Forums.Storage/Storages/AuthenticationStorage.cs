using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using FEwS.Forums.Domain.Authentication;

namespace FEwS.Forums.Storage.Storages;

internal class AuthenticationStorage(
    ForumDbContext dbContext,
    IMapper mapper) : IAuthenticationStorage
{
    public Task<Session?> FindSessionAsync(Guid sessionId, CancellationToken cancellationToken)
    {
        return dbContext.Sessions
            .Where(s => s.SessionId == sessionId)
            .ProjectTo<Session>(mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);
    }
}