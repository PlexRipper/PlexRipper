namespace Reaparr.Data.Configurations;

public class PlexTvShowSeasonConfiguration : IEntityTypeConfiguration<PlexTvShowSeason>
{
    public void Configure(EntityTypeBuilder<PlexTvShowSeason> builder)
    {
        builder.Ignore(x => x.Quality);

        builder.HasIndex(x => new { x.PlexLibraryId, x.SortIndex });

        builder.HasIndex(x => new { x.PlexApiRatingKey, x.PlexServerId });

        builder
            .HasMany(x => x.Qualities)
            .WithOne(x => x.PlexTvShowSeason)
            .HasForeignKey(x => x.PlexTvShowSeasonId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
