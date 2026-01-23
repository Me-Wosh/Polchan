using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Polchan.Core.Threads.Entities;
using Polchan.Core.Users.Entities;
using Thread = Polchan.Core.Threads.Entities.Thread;

namespace Polchan.Infrastructure.Configurations;

public class ThreadConfiguration : IEntityTypeConfiguration<Thread>
{
    public void Configure(EntityTypeBuilder<Thread> builder)
    {
        builder
            .Property(p => p.Name)
            .HasMaxLength(50);

        builder
            .HasMany(t => t.Posts)
            .WithOne(p => p.Thread)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasMany(t => t.Subscribers)
            .WithMany(s => s.SubscribedThreads)
            .UsingEntity<ThreadSubscriptions>(
                r => r
                    .HasOne<User>()
                    .WithMany()
                    .HasForeignKey(ts => ts.SubscriberId)
                    .OnDelete(DeleteBehavior.Restrict),
                l => l
                    .HasOne<Thread>()
                    .WithMany()
                    .HasForeignKey(ts => ts.ThreadId)
                    .OnDelete(DeleteBehavior.Restrict)
            )
            .HasIndex(ts => new { ts.SubscriberId, ts.ThreadId })
            .IsUnique();
            
    }
}
