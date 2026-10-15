namespace Reaparr.Domain;

public class PlexCountry : BaseEntity
{
    public required string Name { get; init; }

    /// <summary>
    /// A MD5 hash of the name.
    /// </summary>
    public required string Key { get; init; }

    public ICollection<PlexLibrary> PlexLibraries { get; set; } = [];

    public ICollection<PlexMovie> PlexMovieCountries { get; set; } = [];

    public ICollection<PlexTvShow> PlexTvShowCountries { get; set; } = [];

    public ICollection<PlexMusicArtist> PlexMusicArtistCountries { get; set; } = [];

    /// <summary>
    /// Gets the album relationships associated with this country.
    /// </summary>
    public ICollection<PlexMusicAlbum> PlexMusicAlbumCountries { get; set; } = [];

    public ICollection<PlexOtherVideo> PlexOtherVideoCountries { get; set; } = [];
}
