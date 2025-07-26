using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlexRipper.Data.Configurations;

public class PlexTvShowEpisodeConfiguration : IEntityTypeConfiguration<PlexTvShowEpisode>
{
    public void Configure(EntityTypeBuilder<PlexTvShowEpisode> builder)
    {
        builder.HasIndex(x => x.SortIndex);

        builder
            .HasMany(x => x.Qualities)
            .WithMany(x => x.TvShowEpisodes)
            .UsingEntity<PlexTvShowEpisodeMediaQuality>(
                l => l.HasOne<PlexMediaQuality>().WithMany().HasForeignKey(e => e.PlexMediaQualityId),
                r => r.HasOne<PlexTvShowEpisode>().WithMany().HasForeignKey(e => e.PlexTvShowEpisodeId)
            );

        builder
            .HasMany(x => x.MediaDataList)
            .WithOne(x => x.PlexTvShowEpisode)
            .HasForeignKey(x => x.PlexTvShowEpisodeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(x => x.TvShowEpisodeMediaQualities)
            .WithOne(x => x.PlexTvShowEpisode)
            .HasForeignKey(x => x.PlexTvShowEpisodeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
