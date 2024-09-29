using PlexRipper.Domain;

namespace Application.Contracts;

public record DownloadWorkerLogDTO
{
    /// <summary>
    /// Gets the date and time when the log entry was created.
    /// </summary>
    public required DateTime CreatedAt { get; init; }

    /// <summary>
    /// Gets the message of the log entry.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Gets the log level of the log entry.
    /// </summary>
    public required NotificationLevel LogLevel { get; init; }

    /// <summary>
    ///  Gets the id of the <see cref="DownloadWorkerTask">DownloadWorkerTask</see> that the log entry belongs to.
    /// </summary>
    public required int DownloadWorkerTaskId { get; init; }

    public required Guid DownloadTaskId { get; init; }
}
