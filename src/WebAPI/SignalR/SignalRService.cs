using Application.Contracts;
using Logging.Interface;
using Microsoft.AspNetCore.SignalR;
using WebAPI.Contracts;

namespace PlexRipper.WebAPI;

/// <summary>
/// A SignalR wrapper to send data to the front-end implementation.
/// </summary>
public class SignalRService : ISignalRService
{
    private readonly ILog _log;
    private readonly IHubContext<ProgressHub, IProgressHub> _progressHub;

    private readonly IHubContext<NotificationHub, INotificationHub> _notificationHub;

    /// <summary>
    /// Initializes a new instance of the <see cref="SignalRService"/> class.
    /// </summary>
    /// <param name="log">The <see cref="ILog"/>.</param>
    /// <param name="progressHub">The <see cref="ProgressHub"/>.</param>
    /// <param name="notificationHub">The <see cref="NotificationHub"/>.</param>
    public SignalRService(
        ILog log,
        IHubContext<ProgressHub, IProgressHub> progressHub,
        IHubContext<NotificationHub, INotificationHub> notificationHub
    )
    {
        _log = log;
        _progressHub = progressHub;
        _notificationHub = notificationHub;
    }

    /// <inheritdoc/>
    public async Task SendLibraryProgressUpdateAsync(LibraryProgress progress)
    {
        await _progressHub.Clients.All.LibraryProgress(progress);
    }

    /// <inheritdoc/>
    public async Task SendDownloadProgressUpdateAsync(
        List<DownloadTaskGeneric> downloadTasks,
        CancellationToken cancellationToken = default
    )
    {
        var update = downloadTasks.ToServerDownloadProgressDTOList();
        if (!update.Any())
        {
            _log.ErrorLine($"Update for ServerDownloadProgress contained no entries to be sent");
            return;
        }

        await _progressHub.Clients.All.ServerDownloadProgress(update.First(), cancellationToken);
    }

    /// <inheritdoc/>
    public async Task SendServerConnectionCheckStatusProgressAsync(ServerConnectionCheckStatusProgress progress)
    {
        await _progressHub.Clients.All.ServerConnectionCheckStatusProgress(progress.ToDTO());
    }

    /// <inheritdoc/>
    public async Task SendFileMergeProgressUpdateAsync(
        FileMergeProgress fileMergeProgress,
        CancellationToken cancellationToken = default
    )
    {
        await _progressHub.Clients.All.FileMergeProgress(fileMergeProgress, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task SendServerSyncProgressUpdateAsync(SyncServerMediaProgress syncServerMediaProgress)
    {
        await _progressHub.Clients.All.SyncServerMediaProgress(syncServerMediaProgress);
    }

    /// <inheritdoc/>
    public async Task SendNotificationAsync(Notification notification)
    {
        await _notificationHub.Clients.All.Notification(notification.ToDTO());
    }

    /// <inheritdoc/>
    public async Task SendRefreshNotificationAsync(DataType dataType, CancellationToken cancellationToken = default)
    {
        await _notificationHub.Clients.All.RefreshNotification(dataType, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task SendRefreshNotificationAsync(
        List<DataType> dataTypes,
        CancellationToken cancellationToken = default
    )
    {
        foreach (var dataType in dataTypes)
            await SendRefreshNotificationAsync(dataType, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task SendJobStatusUpdateAsync<T>(JobStatusUpdate<T> jobStatusUpdate)
        where T : class
    {
        await _progressHub.Clients.All.JobStatusUpdate(jobStatusUpdate.ToDTO());
    }
}
