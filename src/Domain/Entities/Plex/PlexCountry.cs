namespace PlexRipper.Domain;

public class PlexCountry : BaseEntity
{
    public required string Name { get; set; }

    public int PlexKey { get; set; }

    public ICollection<PlexLibrary> PlexLibraries { get; set; } = [];

    public ICollection<PlexMovie> PlexMovieCountries { get; set; } = [];

    public ICollection<PlexTvShow> PlexTvShowCountries { get; set; } = [];
}
