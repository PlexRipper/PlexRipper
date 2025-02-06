namespace PlexRipper.Domain;

public class PlexGenre : BaseEntity
{
    public required string Title { get; set; }

    // public PlexGenreType Type { get; set; }

    public required int Key { get; set; }

    public List<PlexMovie> PlexMovieGenres { get; set; } = [];

    public List<PlexTvShow> PlexTvShowGenres { get; set; } = [];
}
