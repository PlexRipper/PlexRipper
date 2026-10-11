namespace Reaparr.Data;

public class PlexMusicArtistMediaQualityConfiguration : IEntityTypeConfiguration<PlexMusicArtistMediaQuality>
{
    public void Configure(EntityTypeBuilder<PlexMusicArtistMediaQuality> builder)
    {
        builder
            .HasIndex(x => new { x.PlexLibraryId, x.PlexMusicArtistId, x.Quality })
            .IsUnique();
        builder
            .HasOne(x => x.PlexLibrary)
            .WithMany()
            .HasForeignKey(x => x.PlexLibraryId)
            .OnDelete(DeleteBehavior.Cascade);
        builder
            .HasOne(x => x.PlexMusicArtist)
            .WithMany(x => x.Qualities)
            .HasForeignKey(x => x.PlexMusicArtistId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
