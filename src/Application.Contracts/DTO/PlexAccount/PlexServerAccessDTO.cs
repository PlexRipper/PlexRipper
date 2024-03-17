namespace Application.Contracts;

public class PlexServerAccessDTO
{
    public int PlexServerId { get; set; }

    public List<int> PlexLibraryIds { get; set; }
}