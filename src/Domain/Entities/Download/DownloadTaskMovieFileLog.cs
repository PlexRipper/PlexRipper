namespace Reaparr.Domain;

public class DownloadTaskMovieFileLog : DownloadTaskLogBase
{
    #region Relationships

    /// <summary>
    /// Gets the <see cref="DownloadTaskMovieFile"/> this log belongs to.
    /// </summary>
    [Column(Order = 5)]
    public required Guid DownloadTaskFileId { get; init; }

    public DownloadTaskMovieFile? DownloadTaskFile { get; init; }

    /// <summary>
    /// Gets the <see cref="DownloadTaskMovie"/> this log belongs to.
    /// </summary>
    [Column(Order = 6)]
    public required Guid DownloadTaskMovieId { get; init; }

    public DownloadTaskMovie? DownloadTaskMovie { get; init; }

    #endregion
}
