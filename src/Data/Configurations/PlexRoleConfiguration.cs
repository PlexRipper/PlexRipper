using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlexRipper.Data.Configurations;

public class PlexRoleConfiguration : IEntityTypeConfiguration<PlexRole>
{
    public void Configure(EntityTypeBuilder<PlexRole> builder)
    {
        builder.HasIndex(x => x.PlexKey);
    }
}
