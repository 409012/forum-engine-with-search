using FEwS.Forums.Domain.UseCases.SignOn;
using FEwS.Forums.Storage.Storages;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FEwS.Forums.Storage.Tests;

public class SignOnStorageShould(StorageTestFixture fixture) : IClassFixture<StorageTestFixture>
{
    [Fact]
    public async Task StoreNormalizedUserName()
    {
        await using ForumDbContext dbContext = fixture.GetDbContext();
        var sut = new SignOnStorage(dbContext, new GuidFactory());
        string userName = $" Test-{Guid.NewGuid():N} ";

        CreateUserResult result = await sut.CreateUserAsync(userName, "hash", CancellationToken.None);

        Guid userId = result.Should().BeOfType<CreateUserResult.Created>().Which.UserId;
        Entities.User user = await dbContext.Users.SingleAsync(user => user.Id == userId);
        user.NormalizedUserName.Should().Be(userName.Trim().ToUpperInvariant());
    }

    [Theory]
    [InlineData("test", "TEST")]
    [InlineData("test", " test ")]
    [InlineData("тест", "ТЕСТ")]
    public async Task RejectEquivalentUserNames(string firstName, string duplicateName)
    {
        string suffix = Guid.NewGuid().ToString("N");
        await using ForumDbContext dbContext = fixture.GetDbContext();
        var sut = new SignOnStorage(dbContext, new GuidFactory());
        _ = await sut.CreateUserAsync($"{suffix}{firstName.Trim()}", "hash", CancellationToken.None);

        CreateUserResult result = await sut.CreateUserAsync(
            $" {suffix}{duplicateName.Trim()} ", "hash", CancellationToken.None);

        result.Should().BeOfType<CreateUserResult.DuplicateUserName>();
        dbContext.ChangeTracker.Entries<Entities.User>().Should()
            .NotContain(entry => entry.State == EntityState.Added);
    }

    [Fact]
    public async Task CreateOnlyOneUserForConcurrentRegistrations()
    {
        string userName = Guid.NewGuid().ToString("N");
        var start = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        async Task<CreateUserResult> Register(int attempt)
        {
            await using ForumDbContext dbContext = fixture.GetDbContext();
            var sut = new SignOnStorage(dbContext, new GuidFactory());
            await start.Task;
            return await sut.CreateUserAsync(attempt % 2 == 0 ? userName : $" {userName.ToUpperInvariant()} ",
                "hash", CancellationToken.None);
        }

        Task<CreateUserResult>[] requests = Enumerable.Range(0, 8).Select(Register).ToArray();
        start.SetResult();
        CreateUserResult[] results = await Task.WhenAll(requests);

        results.Should().ContainSingle(result => result is CreateUserResult.Created);
        results.Count(result => result is CreateUserResult.DuplicateUserName).Should().Be(7);
        await using ForumDbContext verificationContext = fixture.GetDbContext();
        string normalizedUserName = userName.ToUpperInvariant();
        int count = await verificationContext.Users.CountAsync(user => user.NormalizedUserName == normalizedUserName);
        count.Should().Be(1);
    }

    [Fact]
    public async Task FindRegisteredUserWithEquivalentName()
    {
        await using ForumDbContext dbContext = fixture.GetDbContext();
        string userName = $"user-{Guid.NewGuid():N}";
        var registration = new SignOnStorage(dbContext, new GuidFactory());
        CreateUserResult result = await registration.CreateUserAsync($" {userName} ", "hash", CancellationToken.None);
        Guid userId = result.Should().BeOfType<CreateUserResult.Created>().Which.UserId;
        var signIn = new SignInStorage(new GuidFactory(), dbContext, fixture.GetMapper());

        Domain.Models.User? user = await signIn.FindUserAsync(userName.ToUpperInvariant(), CancellationToken.None);

        user.Should().NotBeNull();
        user.UserId.Should().Be(userId);
    }
}
