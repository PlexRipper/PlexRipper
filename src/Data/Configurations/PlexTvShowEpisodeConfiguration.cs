using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlexRipper.Data.Configurations;

public class PlexTvShowEpisodeConfiguration : IEntityTypeConfiguration<PlexTvShowEpisode>
{
    public void Configure(EntityTypeBuilder<PlexTvShowEpisode> builder)
    {
        // NOTE: This has been added to PlexRipperDbContext.OnModelCreating
        // Based on: https://stackoverflow.com/a/63992731/8205497
        // builder
        //     .Property(x => x.MediaData)
        //     .HasJsonValueConversion();
    }
}