namespace Reaparr.Domain;

public abstract class DownloadTaskParentBase : DownloadTaskBase, IDownloadTaskProgress
{
    /// <summary>
    /// Gets or sets the release year of the media.
    /// </summary>
    [Column(Order = 3)]
    public required int Year { get; init; }

    #region Helpers

    /// <summary>
    /// Gets or sets the total size received of the file in bytes.
    /// </summary>
    [NotMapped]
    public required long DataReceived { get; set; }

    /// <summary>
    /// Gets or sets the total size received of the file in bytes.
    /// </summary>
    [NotMapped]
    public required long FileDataTransferred { get; set; }

    /// <summary>
    /// Gets or sets the total size of the file in bytes.
    /// </summary>
    [NotMapped]
    public required long DataTotal { get; set; }

    /// <summary>
    /// Gets or sets the percentage of the data received from the DataTotal.
    /// </summary>
    [NotMapped]
    public decimal Percentage => DataFormat.GetPercentage(DataReceived, DataTotal);

    /// <summary>
    /// Gets or sets get the download speeds in bytes per second.
    /// </summary>
    [NotMapped]
    public required long DownloadSpeed { get; set; }

    /// <summary>
    /// Gets or sets the file transfer speeds when the finished download is being merged/moved.
    /// </summary>
    [NotMapped]
    public required long FileTransferSpeed { get; set; }

    [NotMapped]
    public long TimeRemaining => DataFormat.GetTimeRemaining(DataTotal - DataReceived, DownloadSpeed);

    #endregion
}
