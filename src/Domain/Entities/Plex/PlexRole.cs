namespace PlexRipper.Domain;

public class PlexRole : BaseEntity
{
    public required int PlexKey { get; set; }

    public required string Name { get; set; }

    public required string ThumbnailUrl { get; set; }

    public List<PlexMovie> PlexMovieRoles { get; set; } = [];

    public List<PlexTvShow> PlexTvShowRoles { get; set; } = [];
}
