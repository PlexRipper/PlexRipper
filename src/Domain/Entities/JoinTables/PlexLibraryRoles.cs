using System.ComponentModel.DataAnnotations.Schema;

namespace PlexRipper.Domain;

public class PlexLibraryRoles
{
    [Column(Order = 1)]
    public int PlexLibraryId { get; set; }

    [Column(Order = 2)]
    public int PlexRoleId { get; set; }
}
