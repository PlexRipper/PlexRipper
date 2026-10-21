namespace Reaparr.Domain;

/// <summary>
/// Links a photo album to an image or clip asset. The two foreign keys form the composite key,
/// allowing one asset to belong to several albums without duplicating the asset record.
/// </summary>
public class PlexPhotoAlbumMembership
{
    /// <summary>
    /// ID of the album containing the asset; part of the composite primary key.
    /// </summary>
    public required int PlexPhotoAlbumId { get; init; }

    /// <summary>
    /// ID of the image or clip asset; part of the composite primary key.
    /// </summary>
    public required int PlexPhotoMediaDataId { get; init; }

    /// <summary>
    /// Navigation to the album containing the asset.
    /// </summary>
    public PlexPhotoAlbum? PlexPhotoAlbum { get; init; }

    /// <summary>
    /// Navigation to the asset shared through this membership.
    /// </summary>
    public PlexPhotoMediaData? PlexPhotoMediaData { get; init; }
}
