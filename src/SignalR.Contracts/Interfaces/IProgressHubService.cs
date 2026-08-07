namespace Reaparr.SignalR.Contracts;

public interface IProgressHubService
{
    /// <summary>
    /// Sends a library progress update to the front-end.
    /// </summary>
    Task SendLibraryProgressUpdateAsync(LibrarySyncProgressDTO progress, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a server connection check status progress update to the front-end.
    /// </summary>
    Task SendServerConnectionCheckStatusProgressAsync(
        ServerConnectionCheckStatusProgress progress,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Sends a background job status update to the front-end.
    /// </summary>
    Task SendJobStatusUpdateAsync<T>(JobStatusUpdate<T> jobStatusUpdate)
        where T : class;

    /// <summary>
    /// Sends an app download progress update to the front-end.
    /// </summary>
    Task SendAppUpdateDownloadProgressAsync(
        AppUpdateDownloadProgressDTO progress,
        CancellationToken cancellationToken = default
    );
    
        
    /// <summary>
    /// Sends a notification that comparison work has settled for one or more libraries.
    /// </summary>
    Task SendLibraryComparisonCompletedAsync(
        LibraryComparisonCompletedDTO notification,
        CancellationToken cancellationToken = default
    );
}
