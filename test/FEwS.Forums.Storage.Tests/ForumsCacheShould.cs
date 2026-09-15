using FEwS.Forums.Domain.Models;
using FEwS.Forums.Storage.Storages;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Time.Testing;
using Xunit;

namespace FEwS.Forums.Storage.Tests;

public sealed class ForumsCacheShould : IDisposable
{
    private readonly FakeTimeProvider timeProvider = new();
    private readonly MemoryCache memoryCache;
    private readonly ForumsCache sut;

    public ForumsCacheShould()
    {
        memoryCache = new MemoryCache(new MemoryCacheOptions
        {
            Clock = new CacheClock(timeProvider),
        });
        sut = new ForumsCache(memoryCache);
    }

    [Fact]
    public async Task ReuseSuccessfulResultUntilExpiration()
    {
        Forum[] original = [new Forum { Id = Guid.NewGuid(), Title = "Original" }];
        Forum[] updated = [new Forum { Id = Guid.NewGuid(), Title = "Updated" }];

        Forum[] first = await sut.GetAsync(_ => Task.FromResult(original), CancellationToken.None);
        timeProvider.Advance(TimeSpan.FromSeconds(9));
        Forum[] cached = await sut.GetAsync(_ => Task.FromResult(updated), CancellationToken.None);
        timeProvider.Advance(TimeSpan.FromSeconds(1));
        Forum[] refreshed = await sut.GetAsync(_ => Task.FromResult(updated), CancellationToken.None);

        first.Should().BeSameAs(original);
        cached.Should().BeSameAs(original);
        refreshed.Should().BeSameAs(updated);
    }

    [Fact]
    public async Task LoadOnlyOnceForConcurrentRequests()
    {
        var completion = new TaskCompletionSource<Forum[]>(TaskCreationOptions.RunContinuationsAsynchronously);
        int calls = 0;

        Task<Forum[]> Load(CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref calls);
            return completion.Task.WaitAsync(cancellationToken);
        }

        Task<Forum[]>[] requests = Enumerable.Range(0, 20)
            .Select(_ => sut.GetAsync(Load, CancellationToken.None)).ToArray();
        Forum[] expected = [new Forum { Id = Guid.NewGuid(), Title = "Forum" }];
        completion.SetResult(expected);
        Forum[][] results = await Task.WhenAll(requests).WaitAsync(TimeSpan.FromSeconds(5));

        calls.Should().Be(1);
        results.Should().OnlyContain(result => ReferenceEquals(result, expected));
    }

    [Fact]
    public async Task RetryAfterFailedLoad()
    {
        var completion = new TaskCompletionSource<Forum[]>(TaskCreationOptions.RunContinuationsAsynchronously);
        Task<Forum[]> first = sut.GetAsync(_ => completion.Task, CancellationToken.None);
        Forum[] expected = [new Forum { Id = Guid.NewGuid(), Title = "Recovered" }];
        Task<Forum[]> second = sut.GetAsync(_ => Task.FromResult(expected), CancellationToken.None);

        completion.SetException(new InvalidOperationException("Load failed"));

        _ = await Assert.ThrowsAsync<InvalidOperationException>(() => first);
        Forum[] result = await second.WaitAsync(TimeSpan.FromSeconds(5));
        result.Should().BeSameAs(expected);
    }

    [Fact]
    public async Task RetryWithWaitingRequestTokenAfterLoaderCancellation()
    {
        using var firstCancellation = new CancellationTokenSource();
        using var secondCancellation = new CancellationTokenSource();
        var completion = new TaskCompletionSource<Forum[]>(TaskCreationOptions.RunContinuationsAsynchronously);
        CancellationToken receivedToken = default;

        Task<Forum[]> first = sut.GetAsync(token => completion.Task.WaitAsync(token), firstCancellation.Token);
        Task<Forum[]> second = sut.GetAsync(token =>
        {
            receivedToken = token;
            return Task.FromResult<Forum[]>([]);
        }, secondCancellation.Token);
        await firstCancellation.CancelAsync();

        _ = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => first);
        Forum[] result = await second.WaitAsync(TimeSpan.FromSeconds(5));
        result.Should().BeEmpty();
        receivedToken.Should().Be(secondCancellation.Token);
    }

    [Fact]
    public async Task CancelWaitingRequestWithoutCancelingLoader()
    {
        using var cancellation = new CancellationTokenSource();
        var completion = new TaskCompletionSource<Forum[]>(TaskCreationOptions.RunContinuationsAsynchronously);
        Task<Forum[]> first = sut.GetAsync(_ => completion.Task, CancellationToken.None);
        bool waitingLoaderCalled = false;
        Task<Forum[]> second = sut.GetAsync(_ =>
        {
            waitingLoaderCalled = true;
            return Task.FromResult<Forum[]>([]);
        }, cancellation.Token);

        await cancellation.CancelAsync();
        _ = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => second);
        first.IsCompleted.Should().BeFalse();
        waitingLoaderCalled.Should().BeFalse();

        completion.SetResult([]);
        _ = await first.WaitAsync(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task ReloadAfterForumCreationInvalidatesCache()
    {
        _ = await sut.GetAsync(_ => Task.FromResult<Forum[]>([]), CancellationToken.None);
        memoryCache.Remove(nameof(GetForumsStorage.GetForumsAsync));
        Forum[] expected = [new Forum { Id = Guid.NewGuid(), Title = "Created" }];

        Forum[] result = await sut.GetAsync(_ => Task.FromResult(expected), CancellationToken.None);

        result.Should().BeSameAs(expected);
    }

    public void Dispose()
    {
        sut.Dispose();
        memoryCache.Dispose();
    }

    private sealed class CacheClock(TimeProvider timeProvider) : ISystemClock
    {
        public DateTimeOffset UtcNow => timeProvider.GetUtcNow();
    }
}
