using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Reaparr.Data.Configurations;

public class PlexServerConfiguration : IEntityTypeConfiguration<PlexServer>
{
    public void Configure(EntityTypeBuilder<PlexServer> builder)
    {
        builder.HasIndex(x => x.MachineIdentifier).IsUnique();

        builder
            .HasMany(x => x.PlexLibraries)
            .WithOne(x => x.PlexServer)
            .HasForeignKey(x => x.PlexServerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(x => x.ServerStatus)
            .WithOne(x => x.PlexServer)
            .HasForeignKey(x => x.PlexServerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(x => x.PlexServerConnections)
            .WithOne(x => x.PlexServer)
            .HasForeignKey(x => x.PlexServerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(x => x.PlexAccountServers)
            .WithOne(x => x.PlexServer)
            .HasForeignKey(x => x.PlexServerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
