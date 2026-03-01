using Reaparr.Domain;

namespace Reaparr.Application.Contracts;

public record DownloadWorkerLogDTO
{
    /// <summary>
    /// Gets the message of the log entry.
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

    /// <summary>
    /// Gets the <see cref="DownloadTaskGeneric"/> this log belongs too
    /// </summary>
    public required Guid DownloadTaskId { get; init; }
}
