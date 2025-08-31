using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Reaparr.Data.Configurations;

public class PlexAccountServerConfiguration : IEntityTypeConfiguration<PlexAccountServer>
{
    public void Configure(EntityTypeBuilder<PlexAccountServer> builder)
    {
        builder.HasKey(bc => new { bc.PlexAccountId, bc.PlexServerId });
    }
}
