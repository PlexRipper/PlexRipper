using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace PlexRipper.Domain;

public class PlexLibraryActors
{
    public PlexLibraryActors() { }

    [SetsRequiredMembers]
    public PlexLibraryActors(int libraryId, int plexActorId)
    {
        PlexLibraryId = libraryId;
        PlexActorId = plexActorId;
    }

    [Column(Order = 1)]
    public required int PlexLibraryId { get; set; }

    [Column(Order = 2)]
    public required int PlexActorId { get; set; }
}
