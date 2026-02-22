using Polchan.Core.Posts.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Polchan.Infrastructure.Data.Configurations;

public class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder
            .Property(p => p.Title)
            .HasMaxLength(100);

        builder
            .Property(p => p.Description)
            .HasMaxLength(255);

        builder
            .HasMany(p => p.Images)
            .WithOne(r => r.Post)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasMany(p => p.Reactions)
            .WithOne(r => r.Post)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasMany(p => p.Comments)
            .WithOne(c => c.Post)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
