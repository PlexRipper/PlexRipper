using System.ComponentModel.DataAnnotations.Schema;

namespace PlexRipper.Domain;

public class PlexTvShowRoles
{
    [Column(Order = 1)]
    public int RolesId { get; set; }

    [Column(Order = 2)]
    public int PlexTvShowId { get; set; }
}
