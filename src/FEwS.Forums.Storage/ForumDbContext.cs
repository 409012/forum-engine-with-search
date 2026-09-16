using FEwS.Forums.Storage.Entities;
using Microsoft.EntityFrameworkCore;

namespace FEwS.Forums.Storage;

public class ForumDbContext(DbContextOptions<ForumDbContext> options) : DbContext(options)
{
    internal const string UserNameIndexName = "UX_Users_NormalizedUserName";

    public DbSet<User> Users { get; set; }
    public DbSet<Forum> Forums { get; set; }
    public DbSet<Topic> Topics { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Session> Sessions { get; set; }
    public DbSet<DomainEvent> DomainEvents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .Property(user => user.NormalizedUserName)
            .HasComputedColumnSql("upper(btrim(\"UserName\"))", stored: true);
        modelBuilder.Entity<User>()
            .HasIndex(user => user.NormalizedUserName)
            .IsUnique()
            .HasDatabaseName(UserNameIndexName);
    }
}
