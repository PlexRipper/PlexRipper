using System.ComponentModel.DataAnnotations.Schema;

namespace PlexRipper.Domain;

public class PlexLibraryGenres
{
    public PlexLibraryGenres() { }

    public PlexLibraryGenres(int plexLibraryId, int plexGenreId)
    {
        PlexLibraryId = plexLibraryId;
        PlexGenreId = plexGenreId;
    }

    [Column(Order = 1)]
    public int PlexLibraryId { get; set; }

    [Column(Order = 2)]
    public int PlexGenreId { get; set; }
}
