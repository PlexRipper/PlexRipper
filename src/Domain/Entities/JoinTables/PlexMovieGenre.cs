namespace PlexRipper.Domain;

public class PlexMovieGenres
{
    public PlexMovieGenres() { }

    public PlexMovieGenres(int genresId, int plexLibraryId, int plexMovieId)
    {
        GenresId = genresId;
        PlexMovieId = plexMovieId;
        PlexLibraryId = plexLibraryId;
    }

    [Column(Order = 1)]
    public int GenresId { get; set; }

    [Column(Order = 2)]
    public int PlexLibraryId { get; set; }

    [Column(Order = 3)]
    public int PlexMovieId { get; set; }
}
