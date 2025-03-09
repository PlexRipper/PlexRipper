using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlexRipper.Data.Configurations;

public class PlexServerStatusConfiguration : IEntityTypeConfiguration<PlexServerStatus>
{
    public void Configure(EntityTypeBuilder<PlexServerStatus> builder)
    {
        builder
            .HasOne(x => x.PlexServer)
            .WithMany(x => x.ServerStatus)
            .HasForeignKey(x => x.PlexServerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.PlexServerConnectionId).IsUnique();
    }
}
