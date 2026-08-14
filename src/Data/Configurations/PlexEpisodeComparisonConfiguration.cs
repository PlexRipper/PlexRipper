namespace Reaparr.Data.Configurations;

public class PlexEpisodeComparisonConfiguration : IEntityTypeConfiguration<PlexEpisodeComparison>
{
    public void Configure(EntityTypeBuilder<PlexEpisodeComparison> builder)
    {
        builder.HasIndex(x => new { x.OwnedPlexLibraryId, x.OwnedPlexMediaId });

        builder.HasIndex(x => new
        {
            x.RemotePlexLibraryId,
            x.OwnedPlexLibraryId,
            x.HitState,
        });

        builder
            .HasIndex(x => new
            {
                x.RemotePlexLibraryId,
                x.OwnedPlexLibraryId,
                x.RemotePlexMediaId,
                x.OwnedPlexMediaId,
            })
            .IsUnique();

        builder
            .HasOne<PlexLibrary>()
            .WithMany()
            .HasForeignKey(x => x.RemotePlexLibraryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne<PlexLibrary>()
            .WithMany()
            .HasForeignKey(x => x.OwnedPlexLibraryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne<PlexTvShowEpisode>()
            .WithMany()
            .HasForeignKey(x => x.RemotePlexMediaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne<PlexTvShowEpisode>()
            .WithMany()
            .HasForeignKey(x => x.OwnedPlexMediaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
