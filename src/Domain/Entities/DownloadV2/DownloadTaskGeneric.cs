namespace PlexRipper.Domain;

public record DownloadTaskGeneric : IDownloadTaskProgress
{
    public Guid Id { get; init; }

    public int Key { get; init; }

    public string Title { get; init; }
    public string FullTitle { get; init; }

    public PlexMediaType MediaType { get; init; }

    public DownloadTaskType DownloadTaskType { get; init; }

    public DownloadStatus DownloadStatus { get; set; }

    public decimal Percentage { get; set; }

    public long DataReceived { get; set; }

    public long DataTotal { get; set; }

    public DateTime CreatedAt { get; init; }

    public string FileName { get; init; }

    public bool IsDownloadable { get; init; }

    /// <summary>
    /// Gets or sets the download directory appended to the MediaPath e.g: [DownloadPath]/[TvShow]/[Season]/ or  [DownloadPath]/[Movie]/.
    /// </summary>
    public string DownloadDirectory { get; init; }

    /// <summary>
    /// Gets or sets the destination directory appended to the MediaPath e.g: [DestinationPath]/[TvShow]/[Season]/ or  [DestinationPath]/[Movie]/.
    /// </summary>
    public string DestinationDirectory { get; init; }

    public string FileLocationUrl { get; set; }

    public long DownloadSpeed { get; set; }

    public long FileTransferSpeed { get; set; }

    #region Relationships

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
}