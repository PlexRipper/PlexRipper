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
    public required DateTime CreatedAt { get; init; }

    /// <summary>
    /// Gets the message of the log entry.
    /// </summary>
    [Column(Order = 2)]
    public required string Message { get; init; }

    /// <summary>
    /// Gets the log level of the log entry.
    /// </summary>
    [Column(Order = 3)]
    public required NotificationLevel LogLevel { get; init; }

    #endregion

    #region Relationships

    /// <summary>
    ///  Gets the <see cref="DownloadWorkerTask">DownloadWorkerTask</see> that the log entry belongs to.
    /// </summary>
    public DownloadWorkerTask DownloadWorkerTask { get; init; }

    /// <summary>
    ///  Gets the id of the <see cref="DownloadWorkerTask">DownloadWorkerTask</see> that the log entry belongs to.
    /// </summary>
    public required int DownloadWorkerTaskId { get; init; }

    #endregion
}
