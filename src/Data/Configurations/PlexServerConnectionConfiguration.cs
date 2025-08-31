using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Reaparr.Data.Configurations;

public class PlexServerConnectionConfiguration : IEntityTypeConfiguration<PlexServerConnection>
{
    public void Configure(EntityTypeBuilder<PlexServerConnection> builder)
    {
        builder
            .HasOne(x => x.LatestConnectionStatus)
            .WithOne(x => x.PlexServerConnection)
            .HasForeignKey<PlexServerStatus>(x => x.PlexServerConnectionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
