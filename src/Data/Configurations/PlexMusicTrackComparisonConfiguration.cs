namespace Reaparr.Data;

public class PlexMusicTrackComparisonConfiguration : IEntityTypeConfiguration<PlexMusicTrackComparison>
{
    public void Configure(EntityTypeBuilder<PlexMusicTrackComparison> builder)
    {
        builder.Property(x => x.HitState).HasDefaultValue(PlexMediaComparisonHitState.Matched);
        builder.Property(x => x.RemoteAudioQuality).HasDefaultValue(AudioQuality.Unknown);
        builder.Property(x => x.OwnedAudioQuality).HasDefaultValue(AudioQuality.Unknown);
        builder.HasIndex(x => new { x.OwnedPlexLibraryId, x.OwnedPlexMediaId });
        builder.HasIndex(x => new
        {
            x.RemotePlexLibraryId, x.OwnedPlexLibraryId, x.RemotePlexMediaId, x.OwnedPlexMediaId,
        }).IsUnique();
        builder.HasOne<PlexLibrary>().WithMany().HasForeignKey(x => x.RemotePlexLibraryId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<PlexLibrary>().WithMany().HasForeignKey(x => x.OwnedPlexLibraryId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<PlexMusicTrack>().WithMany().HasForeignKey(x => x.RemotePlexMediaId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<PlexMusicTrack>().WithMany().HasForeignKey(x => x.OwnedPlexMediaId).OnDelete(DeleteBehavior.Cascade);
    }
}
