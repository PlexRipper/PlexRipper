using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlexRipper.Data.Configurations;

public class PlexMediaQualityConfiguration : IEntityTypeConfiguration<PlexMediaQuality>
{
    public void Configure(EntityTypeBuilder<PlexMediaQuality> builder)
    {
        builder.HasIndex(x => x.Quality);
    }
}
