namespace Reaparr.Application.Contracts;

public interface IDownloadTaskUpdateDispatcher
{
    /// <summary>
    /// Handles a download status change by persisting status updates and scheduling immediate/periodic patch updates.
    /// </summary>
    Task OnStatusChangedAsync(
        DownloadTaskKey key,
        DownloadStatus newStatus,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Handles a download status change and appends an optional error log entry.
    /// </summary>
    Task OnStatusChangedAsync(
        DownloadTaskKey key,
        DownloadStatus newStatus,
        Result? errorResult,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Buffers a progress update for periodic persistence and patch dispatch.
    /// </summary>
    void OnProgressUpdated(DownloadTaskKey key, DownloadTaskProgress progress, DirectDownloadSnapshot? snapshot = null);

    /// <summary>
    /// Queues a scope entry so the next periodic flush sends a patch with the already-persisted file transfer progress.
    /// </summary>
    void NotifyFileTransferProgress(DownloadTaskKey key);
}
