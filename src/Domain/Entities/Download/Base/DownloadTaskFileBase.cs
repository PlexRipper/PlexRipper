using System.ComponentModel.DataAnnotations.Schema;

namespace PlexRipper.Domain;

public abstract class DownloadTaskFileBase : DownloadTaskBase
{
    /// <summary>
    /// Gets or sets the percentage of the data received from the DataTotal.
    /// NOTE: This is calculated at runtime and not stored in the database.
    /// </summary>
    [Column(Order = 4)]
    public decimal Percentage { get; set; }

    /// <summary>
    /// Gets or sets the total size received of the file in bytes.
    /// NOTE: This is calculated at runtime and not stored in the database.
    /// </summary>
    [Column(Order = 5)]
    public long DataReceived { get; set; }

    /// <summary>
    /// Gets or sets the total size of the file in bytes.
    /// NOTE: This is calculated at runtime and not stored in the database.
    /// </summary>
    [Column(Order = 6)]
    public long DataTotal { get; set; }

    /// <summary>
    /// Gets or sets get the download speeds in bytes per second.
    /// NOTE: This is calculated at runtime and not stored in the database.
    /// </summary>
    [Column(Order = 18)]
    public long DownloadSpeed { get; set; }

    /// <summary>
    /// Gets or sets the file transfer speeds when the finished download is being merged/moved.
    /// NOTE: This is calculated at runtime and not stored in the database.
    /// </summary>
    [Column(Order = 19)]
    public long FileTransferSpeed { get; set; }

    [Column(Order = 11)]
    public string FileName { get; set; }

    /// <summary>
    /// Gets or sets the relative obfuscated URL of the media to be downloaded,
    /// e.g: /library/parts/47660/156234666/file.mkv.
    /// </summary>
    [Column(Order = 12)]
    public string FileLocationUrl { get; set; }

    /// <summary>
    /// Gets or sets get or sets the media quality of this <see cref="DownloadTaskGeneric"/>.
    /// </summary>
    [Column(Order = 15)]
    public string Quality { get; set; }

    /// <summary>
    /// Gets or sets the download directory appended to the MediaPath e.g: [DownloadPath]/[TvShow]/[Season]/ or  [DownloadPath]/[Movie]/.
    /// </summary>
    [Column(Order = 16)]
    public string DownloadDirectory { get; set; }

    /// <summary>
    /// Gets or sets the destination directory appended to the MediaPath e.g: [DestinationPath]/[TvShow]/[Season]/ or  [DestinationPath]/[Movie]/.
    /// </summary>
    [Column(Order = 17)]
    public string DestinationDirectory { get; set; }

    #region Relationships

    public List<DownloadWorkerTask> DownloadWorkerTasks { get; set; } = new();

    #endregion

    #region Helpers

    public override void SetNull()
    {
        base.SetNull();
        DownloadWorkerTasks = null;
    }

    public override PlexMediaType MediaType => PlexMediaType.None;

    public override DownloadTaskType DownloadTaskType => DownloadTaskType.None;

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