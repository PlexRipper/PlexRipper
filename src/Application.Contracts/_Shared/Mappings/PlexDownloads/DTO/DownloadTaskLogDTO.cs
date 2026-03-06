using Reaparr.Domain;

namespace Reaparr.Application.Contracts;

public record DownloadTaskLogDTO
{
    /// <summary>
    /// Gets the primary key of the log
    /// </summary>
    public required int Id { get; init; }

    /// <summary>
    /// Gets the status of the download task.
    /// </summary>
    public required DownloadStatus Status { get; init; }

    /// <summary>
    /// Gets the log level of the log entry.
    /// </summary>
    public required NotificationLevel LogLevel { get; init; }

    /// <summary>
    /// Gets the message of the log entry.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Gets the date and time when the log entry was created.
    /// </summary>
    public required DateTime CreatedAt { get; init; }
}
