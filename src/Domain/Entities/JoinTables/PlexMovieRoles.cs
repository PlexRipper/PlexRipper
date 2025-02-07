using System.ComponentModel.DataAnnotations.Schema;

namespace PlexRipper.Domain;

public class PlexMovieRoles
{
    [Column(Order = 1)]
    public int RolesId { get; set; }

    [Column(Order = 2)]
    public int PlexMovieId { get; set; }
}
