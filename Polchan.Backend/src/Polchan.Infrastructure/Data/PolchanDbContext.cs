using Polchan.Core.Posts.Entities;
using Polchan.Core.Resources.Entities;
using Polchan.Core.Users.Entities;
using Microsoft.EntityFrameworkCore;
using Thread = Polchan.Core.Threads.Entities.Thread;
using Polchan.Core.Interfaces;
using Polchan.Core.Primitives;
using Polchan.Core.Threads.Entities;

namespace Polchan.Infrastructure.Data;

public class PolchanDbContext(DbContextOptions<PolchanDbContext> options) : DbContext(options), IPolchanDbContext
{
    public DbSet<Thread> Threads => Set<Thread>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Reaction> Reactions => Set<Reaction>();
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Resource> Resources => Set<Resource>();
    public DbSet<ThreadSubscriptions> ThreadSubscriptions => Set<ThreadSubscriptions>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PolchanDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimeStamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimeStamps()
    {
        var modifiedEntries = ChangeTracker.Entries<BaseEntity>().Where(e => e.State is EntityState.Modified);

        foreach (var modifiedEntry in modifiedEntries)
        {
            modifiedEntry.Entity.ModifiedAt = DateTime.UtcNow;
        }
    }
}
