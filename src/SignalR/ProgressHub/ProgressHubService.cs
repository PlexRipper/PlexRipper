namespace Reaparr.SignalR;

/// <summary>
/// Sends progress-related SignalR messages to the front-end via <see cref="ProgressHub"/>.
/// </summary>
public class ProgressHubService : IProgressHubService
{
    private readonly ILogger _log;
    private readonly IHubContext<ProgressHub, IProgressHub> _hub;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProgressHubService"/> class.
    /// </summary>
    public ProgressHubService(ILogger log, IHubContext<ProgressHub, IProgressHub> hub)
    {
        _log = log.ForContext<ProgressHubService>();
        _hub = hub;
    }

    /// <inheritdoc/>
    public async Task SendLibraryProgressUpdateAsync(
        LibrarySyncProgressDTO progress,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            await _hub.Clients.All.LibraryProgress(progress, cancellationToken);
        }
        catch (Exception ex)
        {
            _log.Here().Warning(ex, "Failed to send library progress update");
        }
    }

    /// <inheritdoc/>
    public async Task SendServerConnectionCheckStatusProgressAsync(
        ServerConnectionCheckStatusProgress progress,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            await _hub.Clients.All.ServerConnectionCheckStatusProgress(progress.ToDTO(), cancellationToken);
        }
        catch (Exception ex)
        {
            _log.Here().Warning(ex, "Failed to send server connection check status progress");
        }
    }

    /// <inheritdoc/>
    public async Task SendJobStatusUpdateAsync<T>(
        JobStatusUpdate<T> jobStatusUpdate,
        CancellationToken cancellationToken = default
    )
        where T : class
    {
        try
        {
            await _hub.Clients.All.JobStatusUpdate(jobStatusUpdate.ToDTO(), cancellationToken);
        }
        catch (Exception ex)
        {
            _log.Here().Warning(ex, "Failed to send job status update");
        }
    }

    /// <inheritdoc/>
    public async Task SendAppUpdateDownloadProgressAsync(
        AppUpdateDownloadProgressDTO progress,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            await _hub.Clients.All.AppUpdateDownloadProgress(progress, cancellationToken);
        }
        catch (Exception ex)
        {
            _log.Here().Warning(ex, "Failed to send app download progress");
        }
    }
    
    /// <inheritdoc/>
    public async Task SendLibraryComparisonCompletedAsync(
        LibraryComparisonCompletedDTO notification,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            await _hub.Clients.All.LibraryComparisonCompleted(notification, cancellationToken);
        }
        catch (Exception ex)
        {
            _log.Here().Warning(ex, "Failed to send library comparison completed notification");
        }
    }

}
