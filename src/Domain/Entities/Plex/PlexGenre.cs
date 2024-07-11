namespace PlexRipper.Domain;

public class PlexGenre : BaseEntity
{
    public required string Tag { get; set; }

    public List<PlexMovieGenre> PlexMovies { get; set; } = new();
}
