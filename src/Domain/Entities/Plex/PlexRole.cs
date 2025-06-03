namespace PlexRipper.Domain;

public class PlexRole : BaseEntity
{
    public required int PlexKey { get; init; }

    public required string Name { get; init; }

    public required string? Role { get; init; }

    public required string TagKey { get; init; }

    public required string? Thumb { get; init; }

    public ICollection<PlexLibrary> PlexLibraries { get; set; } = [];

    public ICollection<PlexMovie> PlexMovieRoles { get; set; } = [];

    public ICollection<PlexTvShow> PlexTvShowRoles { get; set; } = [];
}
