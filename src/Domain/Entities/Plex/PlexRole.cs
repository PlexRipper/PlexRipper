namespace PlexRipper.Domain;

public class PlexRole : BaseEntity
{
    public string Tag { get; set; }

    public virtual List<PlexMovieRole> PlexMovieRoles { get; set; }
}