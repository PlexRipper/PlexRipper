namespace PlexRipper.Domain;

public class PlexTvShowGenre : BaseEntity
{
    public required int PlexGenreId { get; init; }

    public PlexGenre? PlexGenre { get; init; }

    public required int PlexTvShowId { get; init; }

    public PlexTvShow? PlexTvShow { get; init; }
}
