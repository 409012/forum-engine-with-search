using FEwS.Forums.Domain.Models;
using Microsoft.Extensions.Caching.Memory;

namespace FEwS.Forums.Storage.Storages;

internal sealed class ForumsCache(IMemoryCache memoryCache) : IDisposable
{
    private readonly SemaphoreSlim loadingLock = new(1, 1);

    public async Task<Forum[]> GetAsync(
        Func<CancellationToken, Task<Forum[]>> loadForums,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (memoryCache.TryGetValue(nameof(GetForumsStorage.GetForumsAsync), out Forum[]? forums))
        {
            return forums ?? throw new InvalidOperationException();
        }

        await loadingLock.WaitAsync(cancellationToken);
        try
        {
            return await memoryCache.GetOrCreateAsync<Forum[]>(
                nameof(GetForumsStorage.GetForumsAsync),
                async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10);
                    Forum[] loadedForums = await loadForums(cancellationToken);
                    cancellationToken.ThrowIfCancellationRequested();
                    return loadedForums;
                }) ?? throw new InvalidOperationException();
        }
        finally
        {
            _ = loadingLock.Release();
        }
    }

    public void Dispose()
    {
        loadingLock.Dispose();
    }
}
