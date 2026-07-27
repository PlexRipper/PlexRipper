namespace Reaparr.Data.Configurations;

public class PlexTvShowComparisonConfiguration : IEntityTypeConfiguration<PlexTvShowComparison>
{
    public void Configure(EntityTypeBuilder<PlexTvShowComparison> builder)
    {
        builder.HasIndex(x => new { x.OwnedPlexLibraryId, x.OwnedPlexMediaId });

        builder.HasIndex(x => new { x.RemotePlexLibraryId, x.OwnedPlexLibraryId, x.HitState });

        builder.HasIndex(x =>
                new { x.RemotePlexLibraryId, x.OwnedPlexLibraryId, x.RemotePlexMediaId, x.OwnedPlexMediaId }
            )
            .IsUnique();

        builder.HasOne<PlexLibrary>().WithMany().HasForeignKey(x => x.RemotePlexLibraryId).OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<PlexLibrary>().WithMany().HasForeignKey(x => x.OwnedPlexLibraryId).OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<PlexTvShow>().WithMany().HasForeignKey(x => x.RemotePlexMediaId).OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<PlexTvShow>().WithMany().HasForeignKey(x => x.OwnedPlexMediaId).OnDelete(DeleteBehavior.Cascade);
    }
}
