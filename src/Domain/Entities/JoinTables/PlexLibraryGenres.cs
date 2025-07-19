using System.Diagnostics.CodeAnalysis;

namespace PlexRipper.Domain;

public class PlexLibraryGenres
{
    public PlexLibraryGenres() { }

    [SetsRequiredMembers]
    public PlexLibraryGenres(int plexLibraryId, int plexGenreId)
    {
        PlexLibraryId = plexLibraryId;
        PlexGenreId = plexGenreId;
    }

    [Column(Order = 1)]
    public required int PlexLibraryId { get; set; }

    [Column(Order = 2)]
    public required int PlexGenreId { get; set; }
}
