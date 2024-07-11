namespace PlexRipper.Domain;

public class PlexRole : BaseEntity
{
    public required string Tag { get; set; }

    public List<PlexMovieRole> PlexMovieRoles { get; set; } = new();
}
