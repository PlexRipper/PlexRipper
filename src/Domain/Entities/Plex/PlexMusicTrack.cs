namespace Reaparr.Domain;

public class PlexMusicTrack : BasePlexMedia
{
    public override PlexMediaType Type => PlexMediaType.MusicTrack;

    /// <summary>
    /// The Plex key of the <see cref="PlexTvShow"/> this belongs too.
    /// </summary>
    public required int ParentKey { get; set; }

    public required int PlexAlbumId { get; set; }

    public PlexMusicAlbum? PlexAlbum { get; set; }

    /// <summary>
    /// Native Plex track identifier normalized from Plex metadata.
    /// Stores the lowercase identifier suffix without a Plex URI prefix or library-local <see cref="BasePlexMedia.PlexApiRatingKey"/>.
    /// Null when Plex does not provide a valid native track identifier.
    /// </summary>
    public string? PlexApiIdentityId { get; set; }

    public string? MusicBrainzReleaseTrackId { get; set; }

    public int? DiscNumber { get; set; }

    public int? TrackNumber { get; set; }

    public ICollection<PlexMusicTrackMediaData> MediaDataList { get; set; } = [];
}
