namespace PlexRipper.Domain;

public class PlexMovieCountries
{
    public PlexMovieCountries() { }

    public PlexMovieCountries(int countryId, int plexLibraryId, int plexMovieId)
    {
        CountryId = countryId;
        PlexMovieId = plexMovieId;
        PlexLibraryId = plexLibraryId;
    }

    [Column(Order = 1)]
    public int CountryId { get; set; }

    [Column(Order = 2)]
    public int PlexLibraryId { get; set; }

    [Column(Order = 3)]
    public int PlexMovieId { get; set; }
}
