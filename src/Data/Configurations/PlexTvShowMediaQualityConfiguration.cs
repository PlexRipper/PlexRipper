using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlexRipper.Data.Configurations;

public class PlexTvShowMediaQualityConfiguration : IEntityTypeConfiguration<PlexTvShowMediaQuality>
{
    public void Configure(EntityTypeBuilder<PlexTvShowMediaQuality> builder)
    {
        builder.HasKey(bc => new
        {
            bc.PlexMediaQualityId,
            bc.PlexLibraryId,
            bc.PlexTvShowId,
        });
    }
}
