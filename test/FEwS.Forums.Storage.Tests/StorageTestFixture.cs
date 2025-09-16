using Xunit;
using System.Reflection;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Testcontainers.PostgreSql;

namespace FEwS.Forums.Storage.Tests;

public class StorageTestFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer dbContainer = new PostgreSqlBuilder().Build();

    public ForumDbContext GetDbContext()
    {
        return new ForumDbContext(new DbContextOptionsBuilder<ForumDbContext>()
            .UseNpgsql(dbContainer.GetConnectionString())
            .Options);
    }

    public IMapper GetMapper()
    {
        return new Mapper(new MapperConfiguration(cfg =>
            cfg.AddMaps(Assembly.GetAssembly(typeof(ForumDbContext)))));
    }

    public IMemoryCache GetMemoryCache()
    {
        return new MemoryCache(new MemoryCacheOptions());
    }

    public virtual async Task InitializeAsync()
    {
        await dbContainer.StartAsync();
        var forumDbContext = new ForumDbContext(new DbContextOptionsBuilder<ForumDbContext>()
            .UseNpgsql(dbContainer.GetConnectionString()).Options);
        await forumDbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await dbContainer.DisposeAsync();
    }
}