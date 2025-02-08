using System.ComponentModel.DataAnnotations.Schema;

namespace PlexRipper.Domain;

public class PlexTvShowRoles
{
    public PlexTvShowRoles() { }

    public PlexTvShowRoles(int rolesId, int plexLibraryId, int plexTvShowId)
    {
        RolesId = rolesId;
        PlexTvShowId = plexTvShowId;
        PlexLibraryId = plexLibraryId;
    }

    [Column(Order = 1)]
    public int RolesId { get; set; }

    [Column(Order = 2)]
    public int PlexLibraryId { get; set; }

    [Column(Order = 3)]
    public int PlexTvShowId { get; set; }
}
