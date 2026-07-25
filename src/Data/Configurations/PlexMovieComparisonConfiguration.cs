namespace Reaparr.Data.Configurations;

public class PlexMovieComparisonConfiguration : IEntityTypeConfiguration<PlexMovieComparison>
{
    public void Configure(EntityTypeBuilder<PlexMovieComparison> builder)
    {
        builder.HasIndex(x => new { x.OwnedPlexLibraryId, x.OwnedPlexMediaId })
            .HasDatabaseName("IX_PlexMovieComparison_OwnedOwned");

        builder.HasIndex(x => new { x.RemotePlexLibraryId, x.OwnedPlexLibraryId, x.HitState })
            .HasDatabaseName("IX_PlexMovieComparison_RemoteOwnedState");

        builder.HasIndex(x =>
                new { x.RemotePlexLibraryId, x.OwnedPlexLibraryId, x.RemotePlexMediaId, x.OwnedPlexMediaId }
            )
            .HasDatabaseName("UX_PlexMovieComparison_RemoteOwnedMedia")
            .IsUnique();

        builder.HasOne<PlexLibrary>()
            .WithMany()
            .HasForeignKey(x => x.RemotePlexLibraryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<PlexLibrary>()
            .WithMany()
            .HasForeignKey(x => x.OwnedPlexLibraryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<PlexMovie>()
            .WithMany()
            .HasForeignKey(x => x.RemotePlexMediaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<PlexMovie>()
            .WithMany()
            .HasForeignKey(x => x.OwnedPlexMediaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
