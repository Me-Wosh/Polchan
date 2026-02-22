using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Polchan.Core.Resources.Entities;

namespace Polchan.Infrastructure.Data.Configurations;

public class ResourceConfiguration : IEntityTypeConfiguration<Resource>
{
    public void Configure(EntityTypeBuilder<Resource> builder)
    {
        builder
            .HasIndex(r => r.FilePath)
            .IsUnique();
    }
}
