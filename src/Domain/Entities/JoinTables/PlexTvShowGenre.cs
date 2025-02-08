using System.ComponentModel.DataAnnotations.Schema;

namespace PlexRipper.Domain;

public class PlexTvShowGenres
{
    public PlexTvShowGenres() { }

    public PlexTvShowGenres(int genresId, int plexLibraryId, int plexTvShowId)
    {
        GenresId = genresId;
        PlexTvShowId = plexTvShowId;
        PlexLibraryId = plexLibraryId;
    }

    [Column(Order = 1)]
    public int GenresId { get; set; }

    [Column(Order = 2)]
    public int PlexLibraryId { get; set; }

    [Column(Order = 3)]
    public int PlexTvShowId { get; set; }
}
