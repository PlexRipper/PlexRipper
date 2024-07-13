namespace PlexRipper.Domain;

public class PlexTvShowGenre : BaseEntity
{
    public required int PlexGenreId { get; set; }

    public PlexGenre? PlexGenre { get; set; }

    public required int PlexTvShowId { get; set; }

    public PlexTvShow? PlexTvShow { get; set; }
}
