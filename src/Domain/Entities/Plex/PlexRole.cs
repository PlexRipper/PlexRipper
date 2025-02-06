namespace PlexRipper.Domain;

public class PlexRole : BaseEntity
{
    public required int PlexId { get; set; }

    public required string TagKey { get; set; }

    public required string Name { get; set; }

    public required string Role { get; set; }

    public required string ThumbnailUrl { get; set; }

    public List<PlexMovie> PlexMovieRoles { get; set; } = [];

    public List<PlexTvShow> PlexTvShowRoles { get; set; } = [];
}
