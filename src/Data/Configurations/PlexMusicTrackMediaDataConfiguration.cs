namespace Reaparr.Data;

public class PlexMusicTrackMediaDataConfiguration : IEntityTypeConfiguration<PlexMusicTrackMediaData>
{
    public void Configure(EntityTypeBuilder<PlexMusicTrackMediaData> builder)
    {
        builder.Ignore(x => x.Quality);
        builder.Ignore(x => x.VideoResolution);
        builder.Ignore(x => x.VideoCodec);
        builder.HasIndex(x => new { x.PlexLibraryId, x.PlexApiPartId }).IsUnique();
        builder
            .HasIndex(x => new
            {
                x.PlexTrackId,
                x.PlexApiMediaId,
                x.PartIndex,
            })
            .IsUnique();
        builder.HasIndex(x => x.PlexApiRatingKey);
        builder
            .HasOne(x => x.PlexTrack)
            .WithMany(x => x.MediaDataList)
            .HasForeignKey(x => x.PlexTrackId)
            .OnDelete(DeleteBehavior.Cascade);
        builder
            .HasOne(x => x.PlexLibrary)
            .WithMany()
            .HasForeignKey(x => x.PlexLibraryId)
            .OnDelete(DeleteBehavior.Cascade);
        builder
            .HasOne(x => x.PlexServer)
            .WithMany()
            .HasForeignKey(x => x.PlexServerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
