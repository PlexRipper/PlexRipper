namespace Reaparr.Domain;

public class PlexMusicArtist : BasePlexMedia
{
    public override PlexMediaType Type => PlexMediaType.MusicArtist;

    /// <summary>
    /// Native Plex artist identifier extracted from a <c>plex://artist/...</c> metadata GUID.
    /// Stores the lowercase identifier suffix, not the GUID URI or library-local <see cref="BasePlexMedia.PlexApiRatingKey"/>.
    /// Null when Plex does not provide a valid native artist GUID.
    /// </summary>
    public string? PlexApiIdentityId { get; set; }

    public string? MusicBrainzArtistId { get; set; }

    public ICollection<PlexMusicAlbum> Albums { get; set; } = [];

    public ICollection<PlexMusicArtistMediaQuality> Qualities { get; set; } = [];

    public ICollection<PlexActor> Actors { get; init; } = [];

    public ICollection<PlexGenreAudio> Genres { get; init; } = [];

    public ICollection<PlexCountry> Countries { get; init; } = [];
}
