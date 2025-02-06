namespace PlexRipper.Domain;

public class PlexCountry : BaseEntity
{
    public string Name { get; set; }

    public int PlexId { get; set; }

    public List<PlexMovie> PlexMovieCountries { get; set; } = [];

    public List<PlexTvShow> PlexTvShowCountries { get; set; } = [];
}
