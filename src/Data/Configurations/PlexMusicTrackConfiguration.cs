namespace Reaparr.Data;

public class PlexMusicTrackConfiguration : IEntityTypeConfiguration<PlexMusicTrack>
{
    public void Configure(EntityTypeBuilder<PlexMusicTrack> builder)
    {
        // TODO Remove these from the base class
        builder.Ignore(x => x.VideoQuality);
        builder.Ignore(x => x.Studio);
        builder.Ignore(x => x.Guid);
        builder.Ignore(x => x.Guid_IMDB);
        builder.Ignore(x => x.Guid_TMDB);
        builder.Ignore(x => x.Guid_TVDB);
        builder.HasIndex(x => new { x.PlexLibraryId, x.PlexApiRatingKey }).IsUnique();
        builder.Property(x => x.SearchTitle).UseCollation("NOCASE");
        builder.HasIndex(x => new { x.PlexLibraryId, x.SortIndex });
        builder.HasIndex(x => new { x.PlexLibraryId, x.SearchTitle });
        builder.Property(x => x.MusicBrainzReleaseTrackId).UseCollation("BINARY");
        builder.Property(x => x.PlexApiIdentityId).UseCollation("BINARY");
        builder.HasIndex(x => new { x.PlexLibraryId, PlexIdentityId = x.PlexApiIdentityId });

        builder
            .HasOne(x => x.PlexLibrary)
            .WithMany(x => x.Tracks)
            .HasForeignKey(x => x.PlexLibraryId)
            .OnDelete(DeleteBehavior.Cascade);
        builder
            .HasOne(x => x.PlexServer)
            .WithMany()
            .HasForeignKey(x => x.PlexServerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(x => x.PlexAlbum)
            .WithMany(x => x.Tracks)
            .HasForeignKey(x => x.PlexAlbumId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
