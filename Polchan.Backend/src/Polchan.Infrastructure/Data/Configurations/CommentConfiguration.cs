using Polchan.Core.Posts.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Polchan.Infrastructure.Data.Configurations;

public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder
            .Property(c => c.Content)
            .HasMaxLength(255);

        builder
            .HasMany(c => c.Reactions)
            .WithOne(r => r.Comment)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
