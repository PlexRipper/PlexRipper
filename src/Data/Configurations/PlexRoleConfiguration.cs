using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlexRipper.Data.Configurations;

public class PlexRoleConfiguration : IEntityTypeConfiguration<PlexActor>
{
    public void Configure(EntityTypeBuilder<PlexActor> builder)
    {
        builder.HasIndex(x => x.PlexKey).IsUnique();
    }
}
