namespace Reaparr.Data;

public class PlexMusicAlbumConfiguration : IEntityTypeConfiguration<PlexMusicAlbum>
{
    public void Configure(EntityTypeBuilder<PlexMusicAlbum> builder)
    {
        // TODO Remove these from the base class
        builder.Ignore(x => x.Quality);
        builder.Ignore(x => x.Studio);
        builder.Ignore(x => x.Guid);
        builder.Ignore(x => x.Guid_IMDB);
        builder.Ignore(x => x.Guid_TMDB);
        builder.Ignore(x => x.Guid_TVDB);
        builder.HasIndex(x => new { x.PlexLibraryId, x.PlexApiRatingKey }).IsUnique();
        builder.Property(x => x.SearchTitle).UseCollation("NOCASE");
        builder.HasIndex(x => new { x.PlexLibraryId, x.SearchTitle });
        builder.Property(x => x.MusicBrainzReleaseId).UseCollation("BINARY");
        builder.Property(x => x.PlexApiIdentityId).UseCollation("BINARY");
        builder.HasIndex(x => new { x.PlexLibraryId, PlexIdentityId = x.PlexApiIdentityId });
        builder.HasIndex(x => new { x.PlexLibraryId, x.SortIndex });

        builder
            .HasOne(x => x.PlexLibrary)
            .WithMany(x => x.Albums)
            .HasForeignKey(x => x.PlexLibraryId)
            .OnDelete(DeleteBehavior.Cascade);
        builder
            .HasOne(x => x.PlexServer)
            .WithMany()
            .HasForeignKey(x => x.PlexServerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(x => x.PlexArtist)
            .WithMany(x => x.Albums)
            .HasForeignKey(x => x.PlexArtistId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
