namespace Reaparr.SignalR.Contracts;

public interface IProgressHubService
{
    /// <summary>
    /// Sends a library progress update to the front-end.
    /// </summary>
    Task SendLibraryProgressUpdateAsync(LibrarySyncProgressDTO progress);

    /// <summary>
    /// Sends a server connection check status progress update to the front-end.
    /// </summary>
    Task SendServerConnectionCheckStatusProgressAsync(ServerConnectionCheckStatusProgress progress);

    /// <summary>
    /// Sends an app download progress update to the front-end.
    /// </summary>
    Task SendAppUpdateDownloadProgressAsync(AppUpdateDownloadProgressDTO progress);

    /// <summary>
    /// Sends a background job status update to the front-end.
    /// </summary>
    Task SendJobStatusUpdateAsync<T>(JobStatusUpdate<T> jobStatusUpdate)
        where T : class;

    /// <summary>
    /// Sends an integration setup progress update to the front-end.
    /// </summary>
    Task SendIntegrationSetupProgressAsync(IntegrationSetupProgressDTO progress);
}
