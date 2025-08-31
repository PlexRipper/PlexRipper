using Microsoft.AspNetCore.SignalR;
using Reaparr.Application.Contracts;
using Reaparr.Logging;
using Reaparr.WebAPI.Contracts;

namespace Reaparr.WebAPI;

/// <summary>
/// A SignalR wrapper to send data to the front-end implementation.
/// </summary>
public class SignalRService : ISignalRService
{
    private readonly Serilog.ILogger _log;
    private readonly IHubContext<ProgressHub, IProgressHub> _progressHub;

    private readonly IHubContext<NotificationHub, INotificationHub> _notificationHub;

    /// <summary>
    /// Initializes a new instance of the <see cref="SignalRService"/> class.
    /// </summary>
    /// <param name="log">The <see cref="Serilog.ILogger"/>.</param>
    /// <param name="progressHub">The <see cref="ProgressHub"/>.</param>
    /// <param name="notificationHub">The <see cref="NotificationHub"/>.</param>
    public SignalRService(
        Serilog.ILogger log,
        IHubContext<ProgressHub, IProgressHub> progressHub,
        IHubContext<NotificationHub, INotificationHub> notificationHub
    )
    {
        _log = log.ForContext<SignalRService>();
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
            _log.Here().Error($"Update for ServerDownloadProgress contained no entries to be sent");
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
    public async Task SendRefreshNotificationAsync(
        RefreshDataType dataType,
        CancellationToken cancellationToken = default
    )
    {
        await _notificationHub.Clients.All.RefreshNotification(dataType, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task SendRefreshNotificationAsync(
        List<RefreshDataType> dataTypes,
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
