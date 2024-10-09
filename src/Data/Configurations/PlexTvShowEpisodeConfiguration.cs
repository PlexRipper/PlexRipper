using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlexRipper.Data.Configurations;

public class PlexTvShowEpisodeConfiguration : IEntityTypeConfiguration<PlexTvShowEpisode>
{
    public void Configure(EntityTypeBuilder<PlexTvShowEpisode> builder)
    {
    }
}