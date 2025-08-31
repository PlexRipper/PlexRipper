namespace Reaparr.Domain;

public class PlexTvShowCountries
{
    public PlexTvShowCountries() { }

    public PlexTvShowCountries(int countryId, int plexLibraryId, int plexTvShowId)
    {
        CountryId = countryId;
        PlexTvShowId = plexTvShowId;
        PlexLibraryId = plexLibraryId;
    }

    [Column(Order = 1)]
    public int CountryId { get; set; }

    [Column(Order = 2)]
    public int PlexLibraryId { get; set; }

    [Column(Order = 3)]
    public int PlexTvShowId { get; set; }
}
