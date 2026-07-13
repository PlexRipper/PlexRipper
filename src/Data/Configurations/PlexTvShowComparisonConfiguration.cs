namespace Reaparr.Data.Configurations;

public class PlexTvShowComparisonConfiguration : IEntityTypeConfiguration<PlexTvShowComparison>
{
    public void Configure(EntityTypeBuilder<PlexTvShowComparison> builder)
    {
        builder.HasIndex(x => new { x.RemotePlexLibraryId, x.OwnedPlexLibraryId, x.RemotePlexMediaId })
            .HasDatabaseName("IX_PlexTvShowComparison_RemoteOwnedRemote");

        builder.HasIndex(x => new { x.OwnedPlexLibraryId, x.OwnedPlexMediaId })
            .HasDatabaseName("IX_PlexTvShowComparison_OwnedOwned");

        builder.HasIndex(x => new { x.RemotePlexLibraryId, x.OwnedPlexLibraryId, x.HitState })
            .HasDatabaseName("IX_PlexTvShowComparison_RemoteOwnedState");
    }
}
