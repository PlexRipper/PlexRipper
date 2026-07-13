namespace Reaparr.Data.Configurations;

public class PlexEpisodeComparisonConfiguration : IEntityTypeConfiguration<PlexEpisodeComparison>
{
    public void Configure(EntityTypeBuilder<PlexEpisodeComparison> builder)
    {
        builder.HasIndex(x => new { x.RemotePlexLibraryId, x.OwnedPlexLibraryId, x.RemotePlexMediaId })
            .HasDatabaseName("IX_PlexEpisodeComparison_RemoteOwnedRemote");

        builder.HasIndex(x => new { x.OwnedPlexLibraryId, x.OwnedPlexMediaId })
            .HasDatabaseName("IX_PlexEpisodeComparison_OwnedOwned");

        builder.HasIndex(x => new { x.RemotePlexLibraryId, x.OwnedPlexLibraryId, x.HitState })
            .HasDatabaseName("IX_PlexEpisodeComparison_RemoteOwnedState");
    }
}
