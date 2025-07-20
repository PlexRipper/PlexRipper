namespace PlexRipper.Domain;

public abstract class DownloadTaskFileBase : DownloadTaskBase, IDownloadTaskProgress, IDownloadFileTransferProgress
{
    [Column(Order = 11)]
    public required string FileName { get; set; }

    /// <summary>
    /// Gets or sets the relative obfuscated URL of the media to be downloaded,
    /// e.g: /library/parts/47660/156234666/file.mkv.
    /// </summary>
    [Column(Order = 12)]
    public required string FileLocationUrl { get; init; }

    /// <summary>
    /// Gets or sets get or sets the media quality of this <see cref="DownloadTaskGeneric"/>.
    /// </summary>
    [Column(Order = 15)]
    public required string Quality { get; init; }

    [Column(Order = 16)]
    public required DownloadTaskDirectory DirectoryMeta { get; init; }

    #region Download Progress

    /// <summary>
    /// Gets or sets the total size received of the file in bytes.
    /// </summary>
    [Column(Order = 5)]
    public required long DataReceived { get; set; }

    /// <summary>
    /// Gets or sets the total size of the file in bytes.
    /// </summary>
    [Column(Order = 6)]
    public required long DataTotal { get; set; }

    /// <summary>
    /// Gets or sets get the download speeds in bytes per second.
    /// </summary>
    [Column(Order = 18)]
    public required long DownloadSpeed { get; set; }

    #endregion

    #region File Transfer Progress

    /// <summary>
    /// Gets or sets the file transfer speeds when the finished download is being merged/moved.
    /// </summary>
    [Column(Order = 19)]
    public required long FileTransferSpeed { get; set; }

    /// <summary>
    /// Gets or sets the total size received of the file in bytes.
    /// </summary>
    [Column(Order = 20)]
    public required long FileDataTransferred { get; set; }

    /// <summary>
    /// Gets or sets the current file transfer path index used to pause and resume from this file path index.
    /// </summary>
    public int CurrentFileTransferPathIndex { get; set; }

    /// <summary>
    /// Gets or sets the current file transfer bytes offset in combination with the CurrentFileTransferPathIndex used to pause and resume from this offset.
    /// </summary>
    public long CurrentFileTransferBytesOffset { get; set; }

    #endregion

    #region Relationships

    public required ICollection<DownloadWorkerTask> DownloadWorkerTasks { get; set; } = [];

    /// <summary>
    /// Gets or sets the destination folder path id of the <see cref="DownloadTaskFileBase"/>.
    /// This allows the user to pick a destination folder for the download, where the path is copied over once downloading begins.
    /// </summary>
    public required int? DestinationFolderPathId { get; set; }

    #endregion

    #region Helpers

    [NotMapped]
    public override PlexMediaType MediaType => PlexMediaType.None;

    [NotMapped]
    public override DownloadTaskType DownloadTaskType => DownloadTaskType.None;

    [NotMapped]
    public string DestinationFilePath => Path.Join(DestinationDirectory, FileName);

    [NotMapped]
    public List<string> FilePaths => DownloadWorkerTasks.Select(x => x.DownloadFilePath).ToList();

    [NotMapped]
    public decimal Percentage => DownloadTaskPhaseExtensions.Percentage(DownloadTaskPhase, this, this);

    [NotMapped]
    public DownloadTaskPhase DownloadTaskPhase => DownloadStatus.ToDownloadTaskPhase();

    [NotMapped]
    public long Speed => DownloadTaskPhaseExtensions.Speed(DownloadTaskPhase, this, this);

    /// <summary>
    /// Gets the download directory appended to the MediaPath e.g: [DownloadPath]/[TvShow]/[Season]/ or  [DownloadPath]/[Movie]/.
    /// </summary>
    [NotMapped]
    public string DownloadDirectory
    {
        get
        {
            if (DirectoryMeta.DownloadRootPath == string.Empty)
                return string.Empty;

            switch (DownloadTaskType)
            {
                case DownloadTaskType.MovieData:
                    return Path.Combine(DirectoryMeta.DownloadRootPath, "Movies", DirectoryMeta.MovieFolder);
                case DownloadTaskType.EpisodeData:
                    return Path.Combine(
                        DirectoryMeta.DownloadRootPath,
                        "TvShows",
                        DirectoryMeta.TvShowFolder,
                        DirectoryMeta.SeasonFolder
                    );
                default:
                    Result.Fail<string>($"Invalid DownloadTaskType of type: {DownloadTaskType}").LogError();
                    return string.Empty;
            }
        }
    }

    /// <summary>
    /// Gets the destination directory appended to the MediaPath e.g: [DestinationPath]/[TvShow]/[Season]/ or  [DestinationPath]/[Movie]/.
    /// </summary>
    [NotMapped]
    public string DestinationDirectory
    {
        get
        {
            if (DirectoryMeta.DestinationRootPath == string.Empty)
                return string.Empty;

            switch (DownloadTaskType)
            {
                case DownloadTaskType.MovieData:
                    return Path.Combine(DirectoryMeta.DestinationRootPath, DirectoryMeta.MovieFolder);
                case DownloadTaskType.EpisodeData:
                    return Path.Combine(
                        DirectoryMeta.DestinationRootPath,
                        DirectoryMeta.TvShowFolder,
                        DirectoryMeta.SeasonFolder
                    );
                default:
                    Result.Fail<string>($"Invalid DownloadTaskType of type: {DownloadTaskType}").LogError();
                    return string.Empty;
            }
        }
    }

    /// <summary>
    /// Gets a joined string of temp file paths of the <see cref="DownloadWorkerTasks"/> delimited by ";".
    /// </summary>
    [NotMapped]
    public string GetFilePathsCompressed =>
        string.Join(';', DownloadWorkerTasks.Select(x => x.DownloadFilePath).ToArray());

    /// <summary>
    /// Gets the time remaining in seconds the <see cref="DownloadTaskFileBase"/> to finish.
    /// </summary>
    [NotMapped]
    public long TimeRemaining => DownloadTaskPhaseExtensions.TimeRemaining(DownloadTaskPhase, this, this);

    [NotMapped]
    public bool IsSingleFile => DownloadWorkerTasks.Count == 1;

    public override string ToString() =>
        $"[FileMergeProgress {Title} - {Percentage}% - {DataFormat.FormatSpeedString(Speed)} - {DataFormat.FormatSizeString(DownloadTaskPhase == DownloadTaskPhase.FileTransfer ? FileDataTransferred : DataReceived)} / {DataFormat.FormatSizeString(DataTotal)} - {DataFormat.FormatTimeSpanString(TimeSpan.FromSeconds(TimeRemaining))}]";

    public IDownloadFileTransferProgress ToFileTransferProgress() =>
        new DownloadFileTransferProgress
        {
            FileTransferSpeed = FileTransferSpeed,
            FileDataTransferred = FileDataTransferred,
            CurrentFileTransferPathIndex = CurrentFileTransferPathIndex,
            CurrentFileTransferBytesOffset = CurrentFileTransferBytesOffset,
        };

    #endregion
}
