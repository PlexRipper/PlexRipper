namespace Reaparr.Data;

public sealed class PlexPhotoAlbumMembershipConfiguration : IEntityTypeConfiguration<PlexPhotoAlbumMembership>
{
    public void Configure(EntityTypeBuilder<PlexPhotoAlbumMembership> builder)
    {
        builder.HasKey(x => new { x.PlexPhotoAlbumId, x.PlexPhotoMediaDataId });
        builder.HasIndex(x => new { x.PlexPhotoMediaDataId, x.PlexPhotoAlbumId });
        builder
            .HasOne(x => x.PlexPhotoAlbum)
            .WithMany(x => x.Memberships)
            .HasForeignKey(x => x.PlexPhotoAlbumId)
            .OnDelete(DeleteBehavior.Cascade);
        builder
            .HasOne(x => x.PlexPhotoMediaData)
            .WithMany(x => x.AlbumMemberships)
            .HasForeignKey(x => x.PlexPhotoMediaDataId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
