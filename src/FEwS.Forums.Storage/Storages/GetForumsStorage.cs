using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using FEwS.Forums.Domain.Models;
using FEwS.Forums.Domain.UseCases.GetForums;

namespace FEwS.Forums.Storage.Storages;

internal class GetForumsStorage(
    IMemoryCache memoryCache,
    ForumDbContext dbContext,
    IMapper mapper)
    : IGetForumsStorage
{
    public async Task<IEnumerable<Forum>> GetForumsAsync(CancellationToken cancellationToken)
    {
        return await memoryCache.GetOrCreateAsync<Forum[]>(
            nameof(GetForumsAsync),
            entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10);
                return dbContext.Forums
                    .ProjectTo<Forum>(mapper.ConfigurationProvider)
                    .ToArrayAsync(cancellationToken);
            }) ?? throw new InvalidOperationException();
    }
}