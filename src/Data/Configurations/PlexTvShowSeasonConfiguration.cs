using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlexRipper.Data.Configurations;

public class PlexTvShowSeasonConfiguration : IEntityTypeConfiguration<PlexTvShowSeason>
{
    public void Configure(EntityTypeBuilder<PlexTvShowSeason> builder)
    {
        builder.HasIndex(x => x.SortIndex);

        builder
            .HasMany(x => x.Qualities)
            .WithMany(x => x.TvShowSeasons)
            .UsingEntity<PlexTvShowSeasonMediaQuality>(
                l => l.HasOne<PlexMediaQuality>().WithMany().HasForeignKey(e => e.PlexMediaQualityId),
                r => r.HasOne<PlexTvShowSeason>().WithMany().HasForeignKey(e => e.PlexTvShowSeasonId)
            );

        builder
            .HasMany(x => x.PlexTvShowSeasonMediaQualities)
            .WithOne(x => x.PlexTvShowSeason)
            .HasForeignKey(x => x.PlexTvShowSeasonId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
