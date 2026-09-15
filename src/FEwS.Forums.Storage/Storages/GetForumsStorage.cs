using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using FEwS.Forums.Domain.Models;
using FEwS.Forums.Domain.UseCases.GetForums;

namespace FEwS.Forums.Storage.Storages;

internal class GetForumsStorage(
    ForumsCache forumsCache,
    ForumDbContext dbContext,
    IMapper mapper)
    : IGetForumsStorage
{
    public async Task<IEnumerable<Forum>> GetForumsAsync(CancellationToken cancellationToken)
    {
        return await forumsCache.GetAsync(
            token => dbContext.Forums
                .ProjectTo<Forum>(mapper.ConfigurationProvider)
                .ToArrayAsync(token),
            cancellationToken);
    }
}
