namespace PlexRipper.DownloadManager;

public class DownloadProgressDTO
{
    public int Id { get; set; }

    /// <summary>
    /// The formatted media title as shown in Plex.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Note: Naming third just 'type' will cause errors in the Typescript type generating.
    /// </summary>
    public PlexMediaType MediaType { get; set; }

    public string Status { get; set; }

    public decimal Percentage { get; set; }

    public long DataReceived { get; set; }

    public long DataTotal { get; set; }

    public long DownloadSpeed { get; set; }

    public long FileTransferSpeed { get; set; }

    public long TimeRemaining { get; set; }

    public List<string> Actions { get; set; }

    public List<DownloadProgressDTO> Children { get; set; }
}