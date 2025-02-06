namespace PlexRipper.Domain;

public class PlexCountry : BaseEntity
{
    public required string Name { get; set; }

    public required int PlexKey { get; set; }

    public List<PlexMovie> PlexMovieCountries { get; set; } = [];

    public List<PlexTvShow> PlexTvShowCountries { get; set; } = [];
}
