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

    [NotMapped]
    public long DataReceived { get; set; }

    [NotMapped]
    public long DownloadSpeed { get; set; }

    /// <summary>
    /// Calculate properties such as DataReceived, DataTotal based on the nested children.
    /// </summary>
    public abstract void Calculate();

    #endregion
}