namespace PlexRipper.Domain;

public class PlexActor : BaseEntity
{
    public required string Name { get; init; }

    /// <summary>
    /// Gets the unique identifier for the actor in Plex, this is globally unique across Plex Servers and is created by Plex itself.
    /// <example>5d776824103a2d001f563a7e</example>
    /// </summary>
    public required string Key { get; init; }

    public required string? Thumb { get; init; }

    public ICollection<PlexLibrary> PlexLibraries { get; set; } = [];

    public ICollection<PlexMovie> PlexMovieRoles { get; set; } = [];

    public ICollection<PlexTvShow> PlexTvShowRoles { get; set; } = [];
}
