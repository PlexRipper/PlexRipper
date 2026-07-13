namespace Reaparr.Data.Configurations;

public class PlexMovieComparisonConfiguration : IEntityTypeConfiguration<PlexMovieComparison>
{
    public void Configure(EntityTypeBuilder<PlexMovieComparison> builder)
    {
        // Remote-browse + anti-join
        builder.HasIndex(x => new { x.RemotePlexLibraryId, x.OwnedPlexLibraryId, x.RemotePlexMediaId })
            .HasDatabaseName("IX_PlexMovieComparison_RemoteOwnedRemote");

        // Owned-browse (upgrade indicator)
        builder.HasIndex(x => new { x.OwnedPlexLibraryId, x.OwnedPlexMediaId })
            .HasDatabaseName("IX_PlexMovieComparison_OwnedOwned");

        // Remote filter (missing/HQ)
        builder.HasIndex(x => new { x.RemotePlexLibraryId, x.OwnedPlexLibraryId, x.HitState })
            .HasDatabaseName("IX_PlexMovieComparison_RemoteOwnedState");
    }
}
