namespace PlexRipper.Domain;

public class PlexRole : BaseEntity
{
    public required string Name { get; set; }

    public List<PlexLibrary> PlexLibraries { get; set; } = [];

    public List<PlexMovie> PlexMovieRoles { get; set; } = [];

    public List<PlexTvShow> PlexTvShowRoles { get; set; } = [];
}
