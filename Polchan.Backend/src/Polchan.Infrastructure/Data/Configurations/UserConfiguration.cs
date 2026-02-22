using Polchan.Core.Users.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Polchan.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder
            .Property(u => u.Email)
            .HasMaxLength(255);

        builder
            .Property(u => u.Username)
            .HasMaxLength(50);

        builder
            .HasMany(u => u.Posts)
            .WithOne(p => p.Owner)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasMany(u => u.Comments)
            .WithOne(c => c.Owner)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasMany(u => u.Reactions)
            .WithOne(r => r.Owner)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder
            .HasMany(u => u.OwnedThreads)
            .WithOne(t => t.Owner)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
