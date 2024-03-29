using PlexRipper.Domain;

namespace Application.Contracts;

public record DownloadTaskDTO : IDownloadTaskProgress
{
    public required Guid Id { get; set; }

    /// <summary>
    /// The identifier used by Plex to keep track of media.
    /// </summary>
    public required int Key { get; set; }

    /// <summary>
    /// The formatted media title as shown in Plex.
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// The full media title including the [TvShow]/[Season]/[Episode] as shown in Plex.
    /// </summary>
    public required string FullTitle { get; set; }

    /// <summary>
    /// Note: Naming third just 'type' will cause errors in the Typescript type generating.
    /// </summary>
    public required PlexMediaType MediaType { get; set; }

    public required DownloadTaskType DownloadTaskType { get; set; }

    public required DownloadStatus Status { get; set; }

    public required decimal Percentage { get; set; }

    public required long DataReceived { get; set; }

    public required long DataTotal { get; set; }

    public required DateTime CreatedAt { get; init; }

    public required string FileName { get; set; }

    public required long TimeRemaining { get; set; }
    public required string DownloadDirectory { get; set; }

    public required string DestinationDirectory { get; set; }

    /// <summary>
    /// The relative obfuscated URL of the media to be downloaded, e.g: /library/parts/47660/156234666/file.mkv.
    /// </summary>
    public required string FileLocationUrl { get; set; }

    public required long DownloadSpeed { get; set; }

    public required long FileTransferSpeed { get; set; }

    public required string DownloadUrl { get; set; }

    #region Relationships

    public required int PlexServerId { get; set; }

    public required int PlexLibraryId { get; set; }

    public required Guid ParentId { get; set; }

    #endregion

    /// <summary>
    /// The nested <see cref="DownloadTaskGeneric"/> used for seasons and episodes.
    /// "Required = Required.Default" is used for ensuring it's optional in the Typescript generating.
    /// </summary>
    public required List<DownloadTaskDTO> Children { get; set; }

    /// <summary>
    /// The actions that can be taken on this <see cref="DownloadTaskGeneric"/>.
    /// This is filled by the front-end and depends on the DownloadStatus
    /// </summary>
    public required List<string> Actions { get; set; }
}