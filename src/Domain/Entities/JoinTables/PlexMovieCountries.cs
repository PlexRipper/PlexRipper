using System.ComponentModel.DataAnnotations.Schema;

namespace PlexRipper.Domain;

public class PlexMovieCountries
{
    [Column(Order = 1)]
    public int CountryId { get; set; }

    [Column(Order = 2)]
    public int PlexMovieId { get; set; }
}
