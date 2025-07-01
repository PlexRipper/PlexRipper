using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace PlexRipper.Domain;

public class PlexLibraryGenres
{
    public PlexLibraryGenres() { }

    [SetsRequiredMembers]
    public PlexLibraryGenres(int plexLibraryId, int plexGenreId, int plexKey)
    {
        PlexLibraryId = plexLibraryId;
        PlexGenreId = plexGenreId;
        PlexKey = plexKey;
    }

    [Column(Order = 1)]
    public required int PlexLibraryId { get; set; }

    [Column(Order = 2)]
    public required int PlexGenreId { get; set; }

    /// <summary>
    /// The PlexKey is the unique identifier for the genre in Plex in the context of the PlexLibrary.
    /// Meaning it is not globally unique across all Plex servers.
    /// </summary>
    [Column(Order = 3)]
    public required int PlexKey { get; init; }
}
