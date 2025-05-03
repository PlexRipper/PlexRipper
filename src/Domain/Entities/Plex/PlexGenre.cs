namespace PlexRipper.Domain;

public class PlexGenre : BaseEntity
{
    public required string Name { get; set; }

    public long PlexKey { get; set; }

    public ICollection<PlexLibrary> PlexLibraries { get; set; } = [];

    public ICollection<PlexMovie> PlexMovieGenres { get; set; } = [];

    public ICollection<PlexTvShow> PlexTvShowGenres { get; set; } = [];
}
