using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlexRipper.Data.Configurations;

public class PlexTvShowEpisodeMediaQualityConfiguration : IEntityTypeConfiguration<PlexTvShowEpisodeMediaQuality>
{
    public void Configure(EntityTypeBuilder<PlexTvShowEpisodeMediaQuality> builder)
    {
        builder.HasKey(bc => new
        {
            bc.PlexMediaQualityId,
            bc.PlexLibraryId,
            bc.PlexTvShowEpisodeId,
        });
    }
}
