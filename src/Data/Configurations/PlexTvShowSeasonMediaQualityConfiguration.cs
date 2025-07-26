using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlexRipper.Data.Configurations;

public class PlexTvShowSeasonMediaQualityConfiguration : IEntityTypeConfiguration<PlexTvShowSeasonMediaQuality>
{
    public void Configure(EntityTypeBuilder<PlexTvShowSeasonMediaQuality> builder)
    {
        builder.HasKey(bc => new
        {
            bc.PlexMediaQualityId,
            bc.PlexLibraryId,
            bc.PlexTvShowSeasonId,
        });
    }
}
