namespace Reaparr.Domain;

/// <summary>
/// Persisted media-overview snapshot associated with a photo album, separate from the album's asset memberships.
/// </summary>
public class MediaOverviewPhotoAlbumSnapshot : BaseMediaOverviewSnapshot
{
    /// <summary>
    /// ID of the photo album represented by this overview snapshot.
    /// </summary>
    public required int PlexPhotoAlbumId { get; set; }

    /// <summary>
    /// Navigation to the photo album represented by this overview snapshot.
    /// </summary>
    public PlexPhotoAlbum? PlexPhotoAlbum { get; set; }
}
