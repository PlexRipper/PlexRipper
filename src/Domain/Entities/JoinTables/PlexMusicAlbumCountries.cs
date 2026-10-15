namespace Reaparr.Domain;

public class PlexMusicAlbumCountries
{
    public PlexMusicAlbumCountries() { }

    public PlexMusicAlbumCountries(int countryId, int plexLibraryId, int plexMusicAlbumId)
    {
        CountryId = countryId;
        PlexMusicAlbumId = plexMusicAlbumId;
        PlexLibraryId = plexLibraryId;
    }

    /// <summary>
    /// Gets the identifier of the country catalogue entry.
    /// </summary>
    [Column(Order = 1)]
    public int CountryId { get; init; }

    /// <summary>
    /// Gets the identifier of the Plex library containing the album.
    /// </summary>
    [Column(Order = 2)]
    public int PlexLibraryId { get; init; }

    /// <summary>
    /// Gets the identifier of the Plex music album.
    /// </summary>
    [Column(Order = 3)]
    public int PlexMusicAlbumId { get; init; }
}
