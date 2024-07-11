namespace PlexRipper.Domain;

public class PlexGenre : BaseEntity
{
    public string Tag { get; set; }

    public virtual List<PlexMovieGenre> PlexMovies { get; set; }
}
