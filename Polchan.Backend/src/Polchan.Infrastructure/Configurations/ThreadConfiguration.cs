using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
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
    }
}
