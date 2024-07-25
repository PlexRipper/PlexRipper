namespace PlexRipper.Domain;

public class PlexMovieRole : BaseEntity
{
    public required int PlexGenreId { get; init; }

    public PlexGenre? PlexGenre { get; init; }

    public required int PlexMoviesId { get; init; }

    public PlexMovie? PlexMovie { get; init; }
}
