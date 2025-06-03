using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace PlexRipper.Domain;

public class PlexLibraryActors
{
    public PlexLibraryActors() { }

    [SetsRequiredMembers]
    public PlexLibraryActors(int libraryId, int plexActorId, int plexKey)
    {
        PlexLibraryId = libraryId;
        PlexActorId = plexActorId;
        PlexKey = plexKey;
    }

    [Column(Order = 1)]
    public required int PlexLibraryId { get; set; }

    [Column(Order = 2)]
    public required int PlexActorId { get; set; }

    /// <summary>
    /// The PlexKey is the unique identifier for the actor in Plex in the context of the PlexLibrary.
    /// Meaning it is not globally unique across all Plex servers.
    /// </summary>
    [Column(Order = 3)]
    public required int PlexKey { get; init; }
}
