namespace PlexRipper.Domain;

public class PlexGenre : BaseEntity
{
    public required string Tag { get; init; }

    public List<PlexMovieGenre> PlexMovies { get; init; } = new();
}
