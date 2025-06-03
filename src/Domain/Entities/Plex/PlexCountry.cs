namespace PlexRipper.Domain;

public class PlexCountry : BaseEntity
{
    public required string Name { get; init; }

    public required int PlexKey { get; init; }

    public ICollection<PlexLibrary> PlexLibraries { get; set; } = [];

    public ICollection<PlexMovie> PlexMovieCountries { get; set; } = [];

    public ICollection<PlexTvShow> PlexTvShowCountries { get; set; } = [];
}
