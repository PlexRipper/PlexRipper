using PlexRipper.Domain;

namespace DownloadManager.Contracts;

public class DownloadPreviewDTO
{
    public string Title { get; set; }

    public long MediaSize { get; set; }

    public int ChildCount { get; set; }
    public PlexMediaType Type { get; set; }

    public List<DownloadPreviewDTO> Children { get; set; }
}