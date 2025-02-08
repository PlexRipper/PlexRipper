namespace PlexRipper.Domain;

public class PlexGenre : BaseEntity
{
    public required string Name { get; set; }

    public required long PlexKey { get; set; }

    public List<PlexLibrary> PlexLibraries { get; set; } = [];

    public List<PlexMovie> PlexMovieGenres { get; set; } = [];

    public List<PlexTvShow> PlexTvShowGenres { get; set; } = [];
}
