using System.ComponentModel.DataAnnotations.Schema;

namespace PlexRipper.Domain;

public abstract class DownloadTaskFileBase : DownloadTaskBase
{
    /// <summary>
    /// Gets or sets the percentage of the data received from the DataTotal.
    /// NOTE: This is calculated at runtime and not stored in the database.
    /// </summary>
    [Column(Order = 4)]
    public required decimal Percentage { get; set; }

    /// <summary>
    /// Gets or sets the total size received of the file in bytes.
    /// NOTE: This is calculated at runtime and not stored in the database.
    /// </summary>
    [Column(Order = 5)]
    public required long DataReceived { get; set; }

    /// <summary>
    /// Gets or sets the total size of the file in bytes.
    /// NOTE: This is calculated at runtime and not stored in the database.
    /// </summary>
    [Column(Order = 6)]
    public required long DataTotal { get; set; }

    /// <summary>
    /// Gets or sets get the download speeds in bytes per second.
    /// NOTE: This is calculated at runtime and not stored in the database.
    /// </summary>
    [Column(Order = 18)]
    public required long DownloadSpeed { get; set; }

    /// <summary>
    /// Gets or sets the file transfer speeds when the finished download is being merged/moved.
    /// NOTE: This is calculated at runtime and not stored in the database.
    /// </summary>
    [Column(Order = 19)]
    public required long FileTransferSpeed { get; set; }

    [Column(Order = 11)]
    public required string FileName { get; set; }

    /// <summary>
    /// Gets or sets the relative obfuscated URL of the media to be downloaded,
    /// e.g: /library/parts/47660/156234666/file.mkv.
    /// </summary>
    [Column(Order = 12)]
    public required string FileLocationUrl { get; set; }

    /// <summary>
    /// Gets or sets get or sets the media quality of this <see cref="DownloadTaskGeneric"/>.
    /// </summary>
    [Column(Order = 15)]
    public required string Quality { get; set; }

    [Column(Order = 16)]
    public required DownloadTaskDirectory DirectoryMeta { get; set; }

    #region Relationships

    public required List<DownloadWorkerTask> DownloadWorkerTasks { get; set; } = new();

    #endregion

    #region Helpers

    public override PlexMediaType MediaType => PlexMediaType.None;

    public override DownloadTaskType DownloadTaskType => DownloadTaskType.None;

    /// <summary>
    /// Gets the download directory appended to the MediaPath e.g: [DownloadPath]/[TvShow]/[Season]/ or  [DownloadPath]/[Movie]/.
    /// </summary>
    [NotMapped]
    public string DownloadDirectory
    {
        get
        {
            switch (DownloadTaskType)
            {
                case DownloadTaskType.MovieData:
                    return Path.Combine(DirectoryMeta.DownloadRootPath, "Movies", DirectoryMeta.MovieFolder);
                case DownloadTaskType.EpisodeData:
                    return Path.Combine(DirectoryMeta.DownloadRootPath, "TvShows", DirectoryMeta.TvShowFolder, DirectoryMeta.SeasonFolder);
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
    public Result<string> DestinationDirectory => DownloadTaskType switch
    {
        DownloadTaskType.MovieData => Result.Ok(Path.Combine(
            DirectoryMeta.DestinationRootPath,
            DirectoryMeta.MovieFolder)),
        DownloadTaskType.EpisodeData => Result.Ok(Path.Combine(
            DirectoryMeta.DestinationRootPath,
            DirectoryMeta.TvShowFolder,
            DirectoryMeta.SeasonFolder)),
        _ => Result.Fail<string>($"Invalid DownloadTaskType of type: {DownloadTaskType}").LogError(),
    };

    /// <summary>
    /// Gets a joined string of temp file paths of the <see cref="DownloadWorkerTasks"/> delimited by ";".
    /// </summary>
    [NotMapped]
    public string GetFilePathsCompressed => string.Join(';', DownloadWorkerTasks.Select(x => x.TempFilePath).ToArray());

    [NotMapped]
    public string DownloadSpeedFormatted => DataFormat.FormatSpeedString(DownloadSpeed);

    [NotMapped]
    public long TimeRemaining => DataFormat.GetTimeRemaining(DataTotal - DataReceived, DownloadSpeed);

    #endregion
}