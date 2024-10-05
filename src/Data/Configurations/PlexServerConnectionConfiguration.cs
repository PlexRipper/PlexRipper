using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlexRipper.Data.Configurations;

public class PlexServerConnectionConfiguration : IEntityTypeConfiguration<PlexServerConnection>
{
    public void Configure(EntityTypeBuilder<PlexServerConnection> builder)
    {
        builder
            .HasMany(x => x.PlexServerStatus)
            .WithOne(x => x.PlexServerConnection)
            .HasForeignKey(x => x.PlexServerConnectionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
