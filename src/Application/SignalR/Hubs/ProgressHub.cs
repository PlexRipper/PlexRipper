using Microsoft.AspNetCore.SignalR;
using Reaparr.Application.Contracts;

namespace Reaparr.Application;

/// <summary>
///  The ProgressHub class is a SignalR hub that sends progress updates to the front-end.
/// </summary>
public class ProgressHub : Hub<IProgressHub>, IProgressHub
{
    private readonly ILogger _log;

    /// <summary>
    ///  Initializes a new instance of the <see cref="ProgressHub"/> class.
    /// </summary>
    /// <param name="log"> The <see cref="Serilog.ILogger"/> instance to use for logging.</param>
    public ProgressHub(ILogger log)
    {
        _log = log.ForContext<ProgressHub>();
    }

    /// <inheritdoc/>
    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
        _log.Here().Debug("Client connected to {HubName}: {ConnectionId}", nameof(ProgressHub), Context.ConnectionId);
    }

    /// <inheritdoc/>
    public override Task OnDisconnectedAsync(Exception? exception)
    {
        if (exception != null)
        {
            _log.Here()
                .Error(
                    exception,
                    "Client disconnected with error from {HubName}: {ConnectionId}",
                    nameof(ProgressHub),
                    Context.ConnectionId
                );
        }
        else
        {
            _log.Here()
                .Debug("Client disconnected from {HubName}: {ConnectionId}", nameof(ProgressHub), Context.ConnectionId);
        }

        return base.OnDisconnectedAsync(exception);
    }

    /// <inheritdoc/>
    public async Task JobStatusUpdate(JobStatusUpdateDTO jobStatusUpdate, CancellationToken cancellationToken = default)
    {
        _log.Here()
            .Debug(
                "Sending progress: {MessageTypesNotification} => {@JobStatusUpdateDto}",
                MessageTypes.JobStatusUpdate.ToString(),
                jobStatusUpdate
            );
        await Clients.All.JobStatusUpdate(jobStatusUpdate, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task SyncServerMediaProgress(
        SyncServerMediaProgress syncServerMediaProgress,
        CancellationToken cancellationToken = default
    )
    {
        _log.Here()
            .Debug(
                "Sending progress: {MessageTypesNotification} => {@SyncServerProgress}",
                MessageTypes.SyncServerMediaProgress.ToString(),
                syncServerMediaProgress
            );
        await Clients.All.SyncServerMediaProgress(syncServerMediaProgress, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task ServerConnectionCheckStatusProgress(
        ServerConnectionCheckStatusProgressDTO serverConnectionCheckStatusProgress,
        CancellationToken cancellationToken = default
    )
    {
        _log.Here()
            .Debug(
                "Sending progress: {MessageTypesNotification} => {@ServerConnectionCheckStatusProgress}",
                MessageTypes.ServerConnectionCheckStatusProgress.ToString(),
                serverConnectionCheckStatusProgress
            );
        await Clients.All.ServerConnectionCheckStatusProgress(serverConnectionCheckStatusProgress, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task ServerDownloadProgress(
        ServerDownloadProgressDTO serverDownloadProgress,
        CancellationToken cancellationToken = default
    )
    {
        _log.Here()
            .Debug(
                "Sending progress: {MessageTypesNotification} => {@ServerDownloadProgress}",
                MessageTypes.ServerDownloadProgress.ToString(),
                serverDownloadProgress
            );
        await Clients.All.ServerDownloadProgress(serverDownloadProgress, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task DownloadTaskUpdate(DownloadTaskDTO downloadTask, CancellationToken cancellationToken = default)
    {
        _log.Here()
            .Debug(
                "Sending progress: {MessageTypesNotification} => {@DownloadTaskUpdate}",
                MessageTypes.DownloadTaskUpdate.ToString(),
                downloadTask
            );
        await Clients.All.DownloadTaskUpdate(downloadTask, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task LibraryProgress(LibraryProgress libraryProgress, CancellationToken cancellationToken = default)
    {
        _log.Here()
            .Debug(
                "Sending progress: {MessageTypesNotification} => {@LibraryProgress}",
                MessageTypes.LibraryProgress.ToString(),
                libraryProgress
            );
        await Clients.All.LibraryProgress(libraryProgress, cancellationToken);
    }
}
