using FEwS.Forums.Storage.Entities;
using FEwS.Forums.Storage.Storages;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FEwS.Forums.Storage.Tests;

public class SignOutStorageShould(StorageTestFixture fixture) : IClassFixture<StorageTestFixture>
{
    [Fact]
    public async Task RemoveOnlySpecifiedSessionAndAllowRepeatedRemoval()
    {
        await using ForumDbContext dbContext = fixture.GetDbContext();
        var user = new User { Id = Guid.NewGuid(), UserName = Guid.NewGuid().ToString("N"), PasswordHash = "hash" };
        var sessionId = Guid.NewGuid();
        var otherSessionId = Guid.NewGuid();
        dbContext.Sessions.AddRange(
            new Session { SessionId = sessionId, User = user, ExpiresAt = DateTimeOffset.UtcNow.AddHours(1) },
            new Session { SessionId = otherSessionId, User = user, ExpiresAt = DateTimeOffset.UtcNow.AddHours(1) });
        _ = await dbContext.SaveChangesAsync();
        var sut = new SignOutStorage(dbContext);

        await sut.RemoveSessionAsync(sessionId, CancellationToken.None);
        await sut.RemoveSessionAsync(sessionId, CancellationToken.None);

        (await dbContext.Sessions.AnyAsync(session => session.SessionId == sessionId)).Should().BeFalse();
        (await dbContext.Sessions.AnyAsync(session => session.SessionId == otherSessionId)).Should().BeTrue();
        (await dbContext.Users.AnyAsync(existing => existing.Id == user.Id)).Should().BeTrue();
    }
}
