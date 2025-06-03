using System.ComponentModel.DataAnnotations.Schema;

namespace PlexRipper.Domain;

public class PlexLibraryCountries
{
    public PlexLibraryCountries() { }

    public PlexLibraryCountries(int plexLibraryId, int plexCountryId)
    {
        PlexLibraryId = plexLibraryId;
        PlexCountryId = plexCountryId;
    }

    [Column(Order = 1)]
    public int PlexLibraryId { get; set; }

    [Column(Order = 2)]
    public int PlexCountryId { get; set; }
}
