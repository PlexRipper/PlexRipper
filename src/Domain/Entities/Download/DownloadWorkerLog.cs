using System.ComponentModel.DataAnnotations.Schema;

namespace PlexRipper.Domain;

/// <summary>
/// A log entry for the <see cref="DownloadWorkerTask">download worker</see>.
/// </summary>
public class DownloadWorkerLog : BaseEntity
{
    #region Properties

    /// <summary>
    /// Gets the date and time when the log entry was created.
    /// </summary>
    [Column(Order = 1)]
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Gets the log level of the log entry.
    /// </summary>
    [Column(Order = 3)]
    public NotificationLevel LogLevel { get; init; }

    /// <summary>
    /// Gets the message of the log entry.
    /// </summary>
    [Column(Order = 2)]
    public string Message { get; init; }

    #endregion

    #region Relationships

    /// <summary>
    ///  Gets the <see cref="DownloadWorkerTask">DownloadWorkerTask</see> that the log entry belongs to.
    /// </summary>
    public DownloadWorkerTask DownloadTask { get; init; }

    /// <summary>
    ///  Gets the id of the <see cref="DownloadWorkerTask">DownloadWorkerTask</see> that the log entry belongs to.
    /// </summary>
    public int DownloadWorkerTaskId { get; init; }

    #endregion
}