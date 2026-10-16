namespace Reaparr.Data;

public class PlexMovieMediaDataConfiguration : IEntityTypeConfiguration<PlexMovieMediaData>
{
    public void Configure(EntityTypeBuilder<PlexMovieMediaData> builder)
    {
        builder.HasIndex(x => x.VideoQuality);
        builder.HasIndex(x => new { x.PlexMovieId, Quality = x.VideoQuality });
        builder.HasIndex(x => x.PlexApiRatingKey);

        builder
            .HasOne(x => x.PlexMovie)
            .WithMany(x => x.MediaDataList)
            .HasForeignKey(x => x.PlexMovieId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
