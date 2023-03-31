using PlexRipper.Domain;

namespace DownloadManager.Contracts;

public class DownloadMediaDTO
{
    public List<int> MediaIds { get; set; }

    public PlexMediaType Type { get; set; }

    public int PlexServerId { get; set; }

    public int PlexLibraryId { get; set; }
}