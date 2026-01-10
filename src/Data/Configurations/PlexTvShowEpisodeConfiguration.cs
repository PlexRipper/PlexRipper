using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Reaparr.Data.Configurations;

public class PlexTvShowEpisodeConfiguration : IEntityTypeConfiguration<PlexTvShowEpisode>
{
    public void Configure(EntityTypeBuilder<PlexTvShowEpisode> builder)
    {
        builder.HasIndex(x => x.SortIndex);
        builder.HasIndex(x => new { x.TvShowSeasonId, x.SortIndex });
        builder.HasIndex(x => new { x.TvShowId, x.SortIndex });

        builder
            .HasMany(x => x.MediaDataList)
            .WithOne(x => x.PlexTvShowEpisode)
            .HasForeignKey(x => x.PlexTvShowEpisodeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
