using FEwS.Forums.Storage.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;
using Testcontainers.PostgreSql;
using Xunit;

namespace FEwS.Forums.Storage.Tests;

public class UserNameMigrationShould
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task PreserveExistingAccountsAndRejectConflicts(bool hasDuplicates)
    {
        await using PostgreSqlContainer container = new PostgreSqlBuilder().Build();
        await container.StartAsync();
        await using var dbContext = new ForumDbContext(new DbContextOptionsBuilder<ForumDbContext>()
            .UseNpgsql(container.GetConnectionString()).Options);
        IMigrator migrator = dbContext.GetService<IMigrator>();
        await migrator.MigrateAsync("20250916231052_ChangeCommentsBodyToVarchar");
        var firstId = Guid.NewGuid();
        var secondId = Guid.NewGuid();
        dbContext.Users.AddRange(
            new User { Id = firstId, UserName = "legacy", PasswordHash = "first-hash" },
            new User { Id = secondId, UserName = hasDuplicates ? " LEGACY " : "another", PasswordHash = "second-hash" });
        _ = await dbContext.SaveChangesAsync();

        if (hasDuplicates)
        {
            PostgresException exception = await Assert.ThrowsAsync<PostgresException>(() => migrator.MigrateAsync());
            exception.MessageText.Should().Contain("Duplicate normalized user names exist");
        }
        else
        {
            await migrator.MigrateAsync();
            string? normalizedName = await dbContext.Users.AsNoTracking()
                .Where(user => user.Id == firstId)
                .Select(user => user.NormalizedUserName).SingleAsync();
            normalizedName.Should().Be("LEGACY");
        }

        User[] users = await dbContext.Users.AsNoTracking().ToArrayAsync();
        users.Should().HaveCount(2);
        users.Should().Contain(user => user.Id == firstId && user.PasswordHash == "first-hash");
        users.Should().Contain(user => user.Id == secondId && user.PasswordHash == "second-hash");
    }
}
