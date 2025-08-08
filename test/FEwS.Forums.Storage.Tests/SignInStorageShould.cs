using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using FEwS.Forums.Storage.Entities;
using FEwS.Forums.Storage.Storages;

namespace FEwS.Forums.Storage.Tests;

public class SignInStorageFixture : StorageTestFixture
{
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        await using var dbContext = GetDbContext();
        await dbContext.Users.AddRangeAsync(new User
        {
            Id = Guid.Parse("8B41C23E-123E-4F4A-93F0-BEBF9916C8B3"),
            UserName = "testUser",
            PasswordHash = "2"
        }, new User
        {
            Id = Guid.Parse("85895444-65F3-47D8-857D-88F289E83D56"),
            UserName = "another User",
            PasswordHash = "2"
        });
        await dbContext.SaveChangesAsync();
    }
}

public class SignInStorageShould(SignInStorageFixture fixture) : IClassFixture<SignInStorageFixture>
{
    private readonly SignInStorage sut = new(
        new GuidFactory(),
        fixture.GetDbContext(),
        fixture.GetMapper());

    [Fact]
    public async Task ReturnUserWhenDatabaseContainsUserWithSameUserName()
    {
        var actual = await sut.FindUserAsync("testUser", CancellationToken.None);
        actual.Should().NotBeNull();
        actual.UserId.Should().Be(Guid.Parse("8B41C23E-123E-4F4A-93F0-BEBF9916C8B3"));
    }

    [Fact]
    public async Task ReturnNullWhenDatabaseDoesntContainUserWithSameUserName()
    {
        var actual = await sut.FindUserAsync("whatever", CancellationToken.None);
        actual.Should().BeNull();
    }

    [Fact]
    public async Task ReturnNewlyCreatedSessionId()
    {
        var sessionId = await sut.CreateSessionAsync(
            Guid.Parse("8B41C23E-123E-4F4A-93F0-BEBF9916C8B3"),
            new DateTimeOffset(2023, 10, 12, 19, 25, 00, TimeSpan.Zero),
            CancellationToken.None);

        await using var dbContext = fixture.GetDbContext();
        (await dbContext.Sessions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.SessionId == sessionId)).Should().NotBeNull();
    }
}