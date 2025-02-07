using System.ComponentModel.DataAnnotations.Schema;

namespace PlexRipper.Domain;

public class PlexMovieGenres
{
    [Column(Order = 1)]
    public int GenresId { get; set; }

    [Column(Order = 2)]
    public int PlexMovieId { get; set; }
}
