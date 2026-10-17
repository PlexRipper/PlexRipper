namespace Reaparr.Data;

public class PlexTvShowSeasonConfiguration : IEntityTypeConfiguration<PlexTvShowSeason>
{
    public void Configure(EntityTypeBuilder<PlexTvShowSeason> builder)
    {
        builder.Ignore(x => x.VideoQuality);

        builder.HasIndex(x => new { x.PlexLibraryId, x.SortIndex });
        // The leading TvShowId also covers the show-detail lookup for all seasons.
        // SearchTvShowCommandHandler adds SeasonNumber to resolve a specific season.
        builder.HasIndex(x => new { x.TvShowId, x.SeasonNumber });

        builder.HasIndex(x => new { x.PlexApiRatingKey, x.PlexServerId });

        builder
            .HasMany(x => x.Qualities)
            .WithOne(x => x.PlexTvShowSeason)
            .HasForeignKey(x => x.PlexTvShowSeasonId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
