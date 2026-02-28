using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Polchan.Core.Posts.Entities;

namespace Polchan.Infrastructure.Data.Configurations;

public class ReactionConfiguration : IEntityTypeConfiguration<Reaction>
{
    public void Configure(EntityTypeBuilder<Reaction> builder)
    {
        // For post reactions: unique constraint on (OwnerId, PostId) where PostId is not null
        builder.HasIndex(r => new { r.OwnerId, r.PostId })
            .IsUnique()
            .HasFilter("PostId IS NOT NULL");

        // For comment reactions: unique constraint on (OwnerId, CommentId) where CommentId is not null
        builder.HasIndex(r => new { r.OwnerId, r.CommentId })
            .IsUnique()
            .HasFilter("CommentId IS NOT NULL");

        // Ensure that either PostId or CommentId is set, but not both
        builder.ToTable(t => t.HasCheckConstraint("CK_Reaction_PostOrComment", 
            "(PostId IS NOT NULL AND CommentId IS NULL) OR (PostId IS NULL AND CommentId IS NOT NULL)"));
    }
}
