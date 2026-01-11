namespace Reaparr.Domain;

public record DownloadTaskGeneric : IDownloadTaskProgress, IDownloadFileTransferProgress
{
    public required Guid Id { get; init; }

    /// <summary>
    /// The identifier used by Plex to keep track of media.
    /// </summary>
    public required long MediaKey { get; init; }

    /// <summary>
    /// The formatted media title as shown in Plex.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// The full media title including the [TvShow]/[Season]/[Episode] as shown in Plex.
    /// </summary>
    public required string FullTitle { get; init; }

    /// <summary>
    /// Note: Naming third just 'type' will cause errors in the TypeScript type generating.
    /// </summary>
    public required PlexMediaType MediaType { get; init; }

    public required DownloadTaskType DownloadTaskType { get; init; }

    public required DownloadStatus DownloadStatus { get; set; }

    public required DateTime CreatedAt { get; init; }

    public required string FileName { get; init; }

    public required bool IsDownloadable { get; init; }

    /// <summary>
    /// Gets or sets the download directory appended to the MediaPath e.g: [DownloadPath]/[TvShow]/[Season]/ or  [DownloadPath]/[Movie]/.
    /// </summary>
    public required string DownloadDirectory { get; init; }

    public required VideoQuality Quality { get; init; }

    /// <summary>
    /// Gets or sets the destination directory appended to the MediaPath e.g: [DestinationPath]/[TvShow]/[Season]/ or  [DestinationPath]/[Movie]/.
    /// </summary>
    public required string DestinationDirectory { get; init; }

    /// <summary>
    /// The relative obfuscated URL of the media to be downloaded, e.g: /library/parts/47660/156234666/file.mkv.
    /// </summary>
    public required string FileLocationUrl { get; init; }

    #region Download Progress

    public required long DataReceived { get; set; }

    public required long DataTotal { get; set; }

    public required long DownloadSpeed { get; set; }

    #endregion

    #region File Transfer Progress

    public required long FileTransferSpeed { get; set; }

    public required long FileDataTransferred { get; set; }

    public required long CurrentFileTransferBytesOffset { get; set; }

    #endregion

    #region Relationships

    /// <summary>
    /// The nested <see cref="DownloadTaskGeneric"/> used for seasons and episodes.
    /// "Required = Required.Default" is used for ensuring its optional in the TypeScript generating.
    /// </summary>
    public required List<DownloadTaskGeneric> Children { get; set; } = [];

    public required List<DownloadWorkerTask> DownloadWorkerTasks { get; init; } = [];

    public required Guid ParentId { get; init; }

    public required PlexServer? PlexServer { get; init; }

    public required int PlexServerId { get; init; }

    public required PlexLibrary? PlexLibrary { get; init; }

    public required int PlexLibraryId { get; init; }

    #endregion

    #region Helpers

    public DownloadTaskPhase DownloadTaskPhase => DownloadStatus.ToDownloadTaskPhase();

    public decimal Percentage => DownloadTaskPhaseExtensions.Percentage(DownloadTaskPhase, this, this);

    public long Speed => DownloadTaskPhaseExtensions.Speed(DownloadTaskPhase, this, this);

    public long TimeRemaining => DownloadTaskPhaseExtensions.TimeRemaining(DownloadTaskPhase, this, this);

    public DownloadTaskKey ToKey() =>
        new()
        {
            Type = DownloadTaskType,
            Id = Id,
            PlexServerId = PlexServerId,
            PlexLibraryId = PlexLibraryId,
        };

    public override string ToString() =>
        $"[DownloadTaskProgress {Title} - {Percentage}% - {DataFormat.FormatSpeedString(Speed)} - {DataFormat.FormatSizeString(DownloadTaskPhase == DownloadTaskPhase.FileTransfer ? FileDataTransferred : DataReceived)} / {DataFormat.FormatSizeString(DataTotal)} - {DataFormat.FormatTimeSpanString(TimeSpan.FromSeconds(TimeRemaining))}]";

    #endregion
}
