using PlexRipper.Domain;

namespace Application.Contracts;

public record DownloadProgressDTO
{
    public required Guid Id { get; set; }

    /// <summary>
    /// The formatted media title as shown in Plex.
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// Note: Naming third just 'type' will cause errors in the Typescript type generating.
    /// </summary>
    public required PlexMediaType MediaType { get; set; }

    public required DownloadStatus Status { get; set; }

    public required decimal Percentage { get; set; }

    public required long DataReceived { get; set; }

    public required long DataTotal { get; set; }

    public required long DownloadSpeed { get; set; }

    public required long FileTransferSpeed { get; set; }

    public required long TimeRemaining { get; set; }

    public required List<string> Actions { get; set; }

    public required List<DownloadProgressDTO> Children { get; set; }
}
