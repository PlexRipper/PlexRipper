using PlexRipper.DownloadManager;

namespace PlexRipper.WebAPI.SignalR.Common;

public class ServerDownloadProgressDTO
{
    /// <summary>
    /// Gets or sets the <see cref="PlexServer"/> Id.
    /// </summary>

    public int Id { get; set; }

    public List<DownloadProgressDTO> Downloads { get; set; }
}