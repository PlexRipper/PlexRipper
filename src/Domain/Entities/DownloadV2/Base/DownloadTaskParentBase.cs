using System.ComponentModel.DataAnnotations.Schema;

namespace PlexRipper.Domain;

public abstract class DownloadTaskParentBase : DownloadTaskBase, IDownloadTaskProgress
{
    /// <summary>
    /// Gets or sets the media display title.
    /// </summary>
    [Column(Order = 2)]
    public string Title { get; set; }

    /// <summary>
    /// Gets or sets the release year of the media.
    /// </summary>
    [Column(Order = 3)]
    public int Year { get; set; }

    /// <summary>
    /// Gets or sets the full formatted media title, based on the <see cref="PlexMediaType"/>.
    /// E.g. "TvShow/Season/Episode".
    /// </summary>
    [Column(Order = 14)]
    public string FullTitle { get; set; }

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