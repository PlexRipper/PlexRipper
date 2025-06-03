namespace PlexRipper.Domain;

public class PlexCountry : BaseEntity
{
    public required string Name { get; init; }

    /// <summary>
    /// A md5 hash of the name.
    /// </summary>
    public required string Key { get; init; }

    public ICollection<PlexLibrary> PlexLibraries { get; set; } = [];

    public ICollection<PlexMovie> PlexMovieCountries { get; set; } = [];

    public ICollection<PlexTvShow> PlexTvShowCountries { get; set; } = [];
}
