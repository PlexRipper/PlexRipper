namespace PlexRipper.Domain;

public class PlexGenre : BaseEntity
{
    public required string Name { get; set; }

    // public PlexGenreType Type { get; set; }

    public required int PlexKey { get; set; }

    public List<PlexMovie> PlexMovieGenres { get; set; } = [];

    public List<PlexTvShow> PlexTvShowGenres { get; set; } = [];
}
