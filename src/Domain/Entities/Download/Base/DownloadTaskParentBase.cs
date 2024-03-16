using System.ComponentModel.DataAnnotations.Schema;

namespace PlexRipper.Domain;

public abstract class DownloadTaskParentBase : DownloadTaskBase, IDownloadTaskProgress
{
    /// <summary>
    /// Gets or sets the release year of the media.
    /// </summary>
    [Column(Order = 3)]
    public int Year { get; set; }

    #region Helpers

    [NotMapped]
    public decimal Percentage { get; set; }

    /// <summary>
    /// The total size received of the file in bytes.
    /// </summary>
    [NotMapped]
    public long DataReceived { get; set; }

    /// <summary>
    /// The total size of the file in bytes.
    /// </summary>
    [NotMapped]
    public long DataTotal { get; set; }

    [NotMapped]
    public long DownloadSpeed { get; set; }

    [NotMapped]
    public long FileTransferSpeed { get; set; }

    #endregion
}