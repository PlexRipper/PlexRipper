using System.ComponentModel.DataAnnotations.Schema;

namespace PlexRipper.Domain;

public abstract class DownloadTaskParentBase : DownloadTaskBase, IDownloadTaskProgress
{
    /// <summary>
    /// Gets or sets the release year of the media.
    /// </summary>
    [Column(Order = 3)]
    public required int Year { get; set; }

    #region Helpers

    /// <summary>
    /// Gets or sets the percentage of the data received from the DataTotal.
    /// NOTE: This is calculated at runtime and not stored in the database.
    /// </summary>
    [NotMapped]
    public required decimal Percentage { get; set; }

    /// <summary>
    /// Gets or sets the total size received of the file in bytes.
    /// NOTE: This is calculated at runtime and not stored in the database.
    /// </summary>
    [NotMapped]
    public required long DataReceived { get; set; }

    /// <summary>
    /// Gets or sets the total size of the file in bytes.
    /// NOTE: This is calculated at runtime and not stored in the database.
    /// </summary>
    [NotMapped]
    public required long DataTotal { get; set; }

    /// <summary>
    /// Gets or sets get the download speeds in bytes per second.
    /// NOTE: This is calculated at runtime and not stored in the database.
    /// </summary>
    [NotMapped]
    public required long DownloadSpeed { get; set; }

    /// <summary>
    /// Gets or sets the file transfer speeds when the finished download is being merged/moved.
    /// NOTE: This is calculated at runtime and not stored in the database.
    /// </summary>
    [NotMapped]
    public required long FileTransferSpeed { get; set; }

    [NotMapped]
    public long TimeRemaining => DataFormat.GetTimeRemaining(DataTotal - DataReceived, DownloadSpeed);

    #endregion
}
