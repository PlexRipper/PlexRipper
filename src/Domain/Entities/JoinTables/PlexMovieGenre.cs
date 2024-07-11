namespace PlexRipper.Domain;

public class PlexMovieGenre : BaseEntity
{
    public required int PlexGenreId { get; set; }

    public virtual PlexGenre? PlexGenre { get; set; }

    public required int PlexMoviesId { get; set; }

    public virtual PlexMovie? PlexMovie { get; set; }
}
