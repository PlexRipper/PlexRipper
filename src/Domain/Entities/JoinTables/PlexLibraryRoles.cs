using System.ComponentModel.DataAnnotations.Schema;

namespace PlexRipper.Domain;

public class PlexLibraryRoles
{
    public PlexLibraryRoles() { }

    public PlexLibraryRoles(int plexLibraryId, int plexRoleId)
    {
        PlexLibraryId = plexLibraryId;
        PlexRoleId = plexRoleId;
    }

    [Column(Order = 1)]
    public int PlexLibraryId { get; set; }

    [Column(Order = 2)]
    public int PlexRoleId { get; set; }
}
