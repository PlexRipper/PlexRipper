namespace Reaparr.Data.Configurations;

public class PlexSeasonComparisonConfiguration : IEntityTypeConfiguration<PlexSeasonComparison>
{
    public void Configure(EntityTypeBuilder<PlexSeasonComparison> builder)
    {
        builder.HasIndex(x => new { x.RemotePlexLibraryId, x.OwnedPlexLibraryId, x.RemotePlexMediaId })
            .HasDatabaseName("IX_PlexSeasonComparison_RemoteOwnedRemote");

        builder.HasIndex(x => new { x.OwnedPlexLibraryId, x.OwnedPlexMediaId })
            .HasDatabaseName("IX_PlexSeasonComparison_OwnedOwned");

        builder.HasIndex(x => new { x.RemotePlexLibraryId, x.OwnedPlexLibraryId, x.HitState })
            .HasDatabaseName("IX_PlexSeasonComparison_RemoteOwnedState");

        builder.HasIndex(x =>
                new { x.RemotePlexLibraryId, x.OwnedPlexLibraryId, x.RemotePlexMediaId, x.OwnedPlexMediaId }
            )
            .HasDatabaseName("UX_PlexSeasonComparison_RemoteOwnedMedia")
            .IsUnique();

        builder.HasOne<PlexLibrary>().WithMany().HasForeignKey(x => x.RemotePlexLibraryId).OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<PlexLibrary>().WithMany().HasForeignKey(x => x.OwnedPlexLibraryId).OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<PlexTvShowSeason>()
            .WithMany()
            .HasForeignKey(x => x.RemotePlexMediaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<PlexTvShowSeason>()
            .WithMany()
            .HasForeignKey(x => x.OwnedPlexMediaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
