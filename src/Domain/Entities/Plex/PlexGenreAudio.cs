namespace Reaparr.Domain;

public class PlexGenreAudio : BaseEntity
{
    public required string Name { get; init; }

    /// <summary>
    /// A MD5 hash of the name supplied by Plex.
    /// </summary>
    public required string Key { get; init; }

    public ICollection<PlexLibrary> PlexLibraries { get; set; } = [];

    public ICollection<PlexMusicArtist> PlexMusicArtistGenres { get; set; } = [];
}
