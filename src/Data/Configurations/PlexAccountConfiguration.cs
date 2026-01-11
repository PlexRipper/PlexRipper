using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Reaparr.Data.Configurations;

public class PlexAccountConfiguration : IEntityTypeConfiguration<PlexAccount>
{
    public void Configure(EntityTypeBuilder<PlexAccount> builder)
    {
        builder.HasIndex(x => x.Uuid).IsUnique();

        builder.HasMany(x => x.PlexAccountServers).WithOne(x => x.PlexAccount).OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.PlexAccountLibraries).WithOne(x => x.PlexAccount).OnDelete(DeleteBehavior.Cascade);
    }
}
