namespace PlexRipper.Domain;

public class PlexMovieRole : BaseEntity
{
    public required int PlexGenreId { get; set; }

    public PlexGenre? PlexGenre { get; set; }

    public required int PlexMoviesId { get; set; }

    public PlexMovie? PlexMovie { get; set; }
}
