namespace Reaparr.Data;

public class PlexMusicAlbumMediaQualityConfiguration : IEntityTypeConfiguration<PlexMusicAlbumMediaQuality>
{
    public void Configure(EntityTypeBuilder<PlexMusicAlbumMediaQuality> builder)
    {
        builder
            .HasIndex(x => new { x.PlexLibraryId, x.PlexMusicAlbumId, x.Quality })
            .IsUnique();
        builder
            .HasOne(x => x.PlexLibrary)
            .WithMany()
            .HasForeignKey(x => x.PlexLibraryId)
            .OnDelete(DeleteBehavior.Cascade);
        builder
            .HasOne(x => x.PlexMusicAlbum)
            .WithMany(x => x.Qualities)
            .HasForeignKey(x => x.PlexMusicAlbumId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
