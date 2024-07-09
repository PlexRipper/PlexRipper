namespace PlexRipper.Domain;

public record DownloadTaskGeneric : IDownloadTaskProgress
{
    public required Guid Id { get; init; }

    /// <summary>
    /// The identifier used by Plex to keep track of media.
    /// </summary>
    public required int MediaKey { get; init; }

    /// <summary>
    /// The formatted media title as shown in Plex.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// The full media title including the [TvShow]/[Season]/[Episode] as shown in Plex.
    /// </summary>
    public required string FullTitle { get; init; }

    /// <summary>
    /// Note: Naming third just 'type' will cause errors in the Typescript type generating.
    /// </summary>
    public required PlexMediaType MediaType { get; init; }

    public required DownloadTaskType DownloadTaskType { get; init; }

    public required DownloadStatus DownloadStatus { get; set; }

    public required decimal Percentage { get; set; }

    public required long DataReceived { get; set; }

    public required long DataTotal { get; set; }

    public required DateTime CreatedAt { get; init; }

    public required string FileName { get; init; }

    public required bool IsDownloadable { get; init; }

    public required long TimeRemaining { get; init; }

    /// <summary>
    /// Gets or sets the download directory appended to the MediaPath e.g: [DownloadPath]/[TvShow]/[Season]/ or  [DownloadPath]/[Movie]/.
    /// </summary>
    public required string DownloadDirectory { get; set; }

    public required string Quality { get; set; }

    /// <summary>
    /// Gets or sets the destination directory appended to the MediaPath e.g: [DestinationPath]/[TvShow]/[Season]/ or  [DestinationPath]/[Movie]/.
    /// </summary>
    public required string DestinationDirectory { get; set; }

    /// <summary>
    /// The relative obfuscated URL of the media to be downloaded, e.g: /library/parts/47660/156234666/file.mkv.
    /// </summary>
    public required string FileLocationUrl { get; set; }

    public required long DownloadSpeed { get; set; }

    public required long FileTransferSpeed { get; set; }

    #region Relationships

    /// <summary>
    /// The nested <see cref="DownloadTaskGeneric"/> used for seasons and episodes.
    /// "Required = Required.Default" is used for ensuring its optional in the Typescript generating.
    /// </summary>
    public required List<DownloadTaskGeneric> Children { get; set; } = new();

    public required List<DownloadWorkerTask> DownloadWorkerTasks { get; set; } = new();

    public required Guid ParentId { get; init; }

    public required PlexServer PlexServer { get; init; }

    public required int PlexServerId { get; init; }

    public required PlexLibrary PlexLibrary { get; init; }

    public required int PlexLibraryId { get; init; }

    #endregion

    public DownloadTaskKey ToKey() => new()
    {
        Type = DownloadTaskType,
        Id = Id,
        PlexServerId = PlexServerId,
        PlexLibraryId = PlexLibraryId,
    };

    public override string ToString() => $"DownloadTaskUpdate: [{DownloadTaskType}] [{DownloadStatus}] [{Title}] [Download Location: {DownloadDirectory}]";
}