namespace PlexRipper.Domain;

public class PlexRole : BaseEntity
{
    public required string Name { get; set; }

    public ICollection<PlexLibrary> PlexLibraries { get; set; } = [];

    public ICollection<PlexMovie> PlexMovieRoles { get; set; } = [];

    public ICollection<PlexTvShow> PlexTvShowRoles { get; set; } = [];
}
