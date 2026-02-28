using Microsoft.EntityFrameworkCore;
using Polchan.Core.Posts.Entities;
using Polchan.Core.Resources.Entities;
using Polchan.Core.Users.Entities;
using Thread = Polchan.Core.Threads.Entities.Thread;

namespace Polchan.Core.Interfaces;

public interface IPolchanDbContext
{
    DbSet<Thread> Threads { get; }
    DbSet<Post> Posts { get; }
    DbSet<Comment> Comments { get; }
    DbSet<Reaction> Reactions { get; }
    DbSet<User> Users { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<Resource> Resources { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
