namespace Reaparr.Domain;

public class PlexMusicAlbum : BasePlexMedia
{
    public override PlexMediaType Type => PlexMediaType.MusicAlbum;

    /// <summary>
    /// The Plex key of the <see cref="PlexTvShow"/> this belongs too.
    /// </summary>
    public required int ParentKey { get; set; }

    public required int PlexArtistId { get; set; }

    public PlexMusicArtist? PlexArtist { get; set; }

    /// <summary>
    /// Native Plex album identifier extracted from a <c>plex://album/...</c> metadata GUID.
    /// Stores the lowercase identifier suffix, not the GUID URI or library-local <see cref="BasePlexMedia.PlexApiRatingKey"/>.
    /// Null when Plex does not provide a valid native album GUID.
    /// </summary>
    public required string? PlexApiIdentityId { get; set; }

    public required string? MusicBrainzReleaseId { get; set; }

    public DateTime? ReleaseDate { get; set; }

    public required string? RecordLabel { get; set; }

    /// <summary>
    /// Gets the countries associated with this album from Plex metadata.
    /// </summary>
    public ICollection<PlexCountry> Countries { get; init; } = [];

    public required int? DiscCount { get; set; }

    public required int? TrackCount { get; set; }

    public ICollection<PlexMusicTrack> Tracks { get; set; } = [];

    public ICollection<PlexMusicAlbumMediaQuality> Qualities { get; set; } = [];
}
