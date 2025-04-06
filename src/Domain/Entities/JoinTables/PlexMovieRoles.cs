using System.ComponentModel.DataAnnotations.Schema;

namespace PlexRipper.Domain;

public class PlexMovieRoles
{
    public PlexMovieRoles() { }

    public PlexMovieRoles(int rolesId, int plexLibraryId, int plexMovieId)
    {
        RolesId = rolesId;
        PlexMovieId = plexMovieId;
        PlexLibraryId = plexLibraryId;
    }

    [Column(Order = 1)]
    public int RolesId { get; set; }

    [Column(Order = 2)]
    public int PlexLibraryId { get; set; }

    [Column(Order = 3)]
    public int PlexMovieId { get; set; }
}
