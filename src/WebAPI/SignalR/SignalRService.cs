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

    #region ProgressHub

    public async Task SendLibraryProgressUpdateAsync(int id, int received, int total, bool isRefreshing = true)
    {
        var libraryProgress = new LibraryProgress(id, received, total, isRefreshing);
        await _progressHub.Clients.All.LibraryProgress(libraryProgress);
    }

    #region DownloadProgress

    public async Task SendDownloadProgressUpdateAsync(
        int plexServerId,
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

    #endregion

    public async Task SendServerInspectStatusProgressAsync(InspectServerProgress progress)
    {
        var progressDTO = new InspectServerProgressDTO
        {
            PlexServerId = progress.PlexServerId,
            RetryAttemptIndex = progress.RetryAttemptIndex,
            RetryAttemptCount = progress.RetryAttemptCount,
            TimeToNextRetry = progress.TimeToNextRetry,
            StatusCode = progress.StatusCode,
            ConnectionSuccessful = progress.ConnectionSuccessful,
            Completed = progress.Completed,
            Message = progress.Message,
            PlexServerConnection = progress.PlexServerConnection.ToDTO(),
        };
        await _progressHub.Clients.All.InspectServerProgress(progressDTO);
    }

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

    public async Task SendServerSyncProgressUpdateAsync(SyncServerProgress syncServerProgress)
    {
        await _progressHub.Clients.All.SyncServerProgress(syncServerProgress);
    }

    #endregion

    #region NotificationHub

    public async Task SendNotificationAsync(Notification notification)
    {
        await _notificationHub.Clients.All.Notification(notification.ToDTO());
    }

    #endregion

    #region JobStateNotification

    public async Task SendJobStatusUpdateAsync(JobStatusUpdate jobStatusUpdate)
    {
        await _progressHub.Clients.All.JobStatusUpdate(jobStatusUpdate.ToDTO());
    }

    #endregion
}
