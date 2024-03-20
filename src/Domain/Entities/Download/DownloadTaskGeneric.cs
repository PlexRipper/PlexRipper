namespace PlexRipper.Domain;

public record DownloadTaskGeneric : IDownloadTaskProgress
{
    public Guid Id { get; init; }

    /// <summary>
    /// The identifier used by Plex to keep track of media.
    /// </summary>
    public int Key { get; init; }

    /// <summary>
    /// The formatted media title as shown in Plex.
    /// </summary>
    public string Title { get; init; }

    /// <summary>
    /// The full media title including the [TvShow]/[Season]/[Episode] as shown in Plex.
    /// </summary>
    public string FullTitle { get; init; }

    /// <summary>
    /// Note: Naming third just 'type' will cause errors in the Typescript type generating.
    /// </summary>
    public PlexMediaType MediaType { get; init; }

    public DownloadTaskType DownloadTaskType { get; init; }

    public DownloadStatus DownloadStatus { get; set; }

    public decimal Percentage { get; set; }

    public long DataReceived { get; set; }

    public long DataTotal { get; set; }

    public DateTime CreatedAt { get; init; }

    public string FileName { get; init; }

    public bool IsDownloadable { get; init; }

    public long TimeRemaining { get; init; }

    /// <summary>
    /// Gets or sets the download directory appended to the MediaPath e.g: [DownloadPath]/[TvShow]/[Season]/ or  [DownloadPath]/[Movie]/.
    /// </summary>
    public string DownloadDirectory { get; init; }

    /// <summary>
    /// Gets or sets the destination directory appended to the MediaPath e.g: [DestinationPath]/[TvShow]/[Season]/ or  [DestinationPath]/[Movie]/.
    /// </summary>
    public string DestinationDirectory { get; init; }

    /// <summary>
    /// The relative obfuscated URL of the media to be downloaded, e.g: /library/parts/47660/156234666/file.mkv.
    /// </summary>
    public string FileLocationUrl { get; set; }

    public long DownloadSpeed { get; set; }

    public long FileTransferSpeed { get; set; }

    #region Relationships

    /// <summary>
    /// The nested <see cref="DownloadTaskGeneric"/> used for seasons and episodes.
    /// "Required = Required.Default" is used for ensuring it's optional in the Typescript generating.
    /// </summary>
    public List<DownloadTaskGeneric> Children { get; set; }

    public List<DownloadWorkerTask> DownloadWorkerTasks { get; set; }

    public Guid ParentId { get; init; }

    public PlexServer PlexServer { get; init; }

    public int PlexServerId { get; init; }

    public PlexLibrary PlexLibrary { get; init; }

    public int PlexLibraryId { get; init; }

    #endregion

    public DownloadTaskKey ToKey() => new(DownloadTaskType, Id, PlexServerId, PlexLibraryId);

    public DownloadTaskKey? ToParentKey()
    {
        return DownloadTaskType switch
        {
            DownloadTaskType.Movie => null,
            DownloadTaskType.MovieData => new DownloadTaskKey(DownloadTaskType.Movie, ParentId, PlexServerId, PlexLibraryId),
            DownloadTaskType.MoviePart => new DownloadTaskKey(DownloadTaskType.Movie, ParentId, PlexServerId, PlexLibraryId),
            DownloadTaskType.TvShow => null,
            DownloadTaskType.Season => new DownloadTaskKey(DownloadTaskType.TvShow, ParentId, PlexServerId, PlexLibraryId),
            DownloadTaskType.Episode => new DownloadTaskKey(DownloadTaskType.Season, ParentId, PlexServerId, PlexLibraryId),
            DownloadTaskType.EpisodeData => new DownloadTaskKey(DownloadTaskType.Episode, ParentId, PlexServerId, PlexLibraryId),
            DownloadTaskType.EpisodePart => new DownloadTaskKey(DownloadTaskType.Episode, ParentId, PlexServerId, PlexLibraryId),
            _ => null,
        };
    }

    public override string ToString() => $"[DownloadTask [{DownloadTaskType}] [{DownloadStatus}] [{Title}]";
}