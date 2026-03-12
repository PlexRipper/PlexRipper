namespace Reaparr.Domain;

public abstract class DownloadTaskParentBase : DownloadTaskBase, IDownloadTaskProgress
{
    /// <summary>
    /// Gets or sets the release year of the media.
    /// </summary>
    [Column(Order = 8)]
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
    public decimal Percentage { get; set; }

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
    public int TimeRemaining { get; set; }

    #endregion
}
