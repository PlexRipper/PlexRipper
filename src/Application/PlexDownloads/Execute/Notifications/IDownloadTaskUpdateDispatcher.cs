using Reaparr.Domain;

namespace Reaparr.Application;

public interface IDownloadTaskUpdateDispatcher
{
    /// <summary>
    /// Handles a download status change by persisting status updates and scheduling immediate/periodic patch updates.
    /// </summary>
    Task<Result> OnStatusChangedAsync(
        DownloadTaskKey key,
        DownloadStatus newStatus,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Handles a download status change and appends an optional error log entry.
    /// </summary>
    Task<Result> OnStatusChangedAsync(
        DownloadTaskKey key,
        DownloadStatus newStatus,
        Result? errorResult,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Buffers a progress update for periodic persistence and patch dispatch.
    /// </summary>
    Result OnProgressUpdated(
        DownloadTaskKey key,
        DownloadTaskProgress progress,
        DirectDownloadSnapshot? snapshot = null
    );
}
