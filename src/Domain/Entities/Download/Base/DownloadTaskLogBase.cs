namespace Reaparr.Domain;

/// <summary>
/// A log entry for child classes of the <see cref="DownloadTaskFileBase"/>.
/// </summary>
public abstract class DownloadTaskLogBase : BaseEntity
{
    #region Properties

    /// <summary>
    /// Gets the status of the log entry.
    /// </summary>
    [Column(Order = 1)]
    public required DownloadStatus Status { get; init; }

    /// <summary>
    /// Gets the log level of the log entry.
    /// </summary>
    [Column(Order = 2)]
    public required NotificationLevel LogLevel { get; init; }

    /// <summary>
    /// Gets the message of the log entry.
    /// </summary>
    [Column(Order = 3)]
    public required string Message { get; init; }

    /// <summary>
    /// Gets the date and time when the log entry was created.
    /// </summary>
    [Column(Order = 4)]
    public required DateTime CreatedAt { get; init; }
    #endregion
}
