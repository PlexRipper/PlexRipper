using Application.Contracts;
using Logging.Interface;
using Microsoft.AspNetCore.SignalR;
using WebAPI.Contracts;

namespace PlexRipper.WebAPI;

public class ProgressHub : Hub<IProgressHub>
{
    private readonly ILog<ProgressHub> _log;

    public ProgressHub(ILog<ProgressHub> log)
    {
        _log = log;
    }

    public async Task JobStatusUpdate(JobStatusUpdateDTO jobStatusUpdate, CancellationToken cancellationToken = default)
    {
        _log.Debug(
            "Sending progress: {MessageTypesNotification} => {@JobStatusUpdateDto}",
            MessageTypes.JobStatusUpdate.ToString(),
            jobStatusUpdate
        );
        await Clients.All.JobStatusUpdate(jobStatusUpdate, cancellationToken);
    }

    public async Task JobStatusUpdate<T>(
        JobStatusUpdateDTO<T> jobStatusUpdate,
        CancellationToken cancellationToken = default
    )
        where T : class
    {
        _log.Debug(
            "Sending progress: {MessageTypesNotification} => {@JobStatusUpdateDto}",
            MessageTypes.JobStatusUpdate.ToString(),
            jobStatusUpdate
        );
        await Clients.All.JobStatusUpdate(jobStatusUpdate, cancellationToken);
    }

    public async Task SyncServerProgress(
        SyncServerProgress syncServerProgress,
        CancellationToken cancellationToken = default
    )
    {
        _log.Debug(
            "Sending progress: {MessageTypesNotification} => {@SyncServerProgress}",
            MessageTypes.SyncServerProgress.ToString(),
            syncServerProgress
        );
        await Clients.All.SyncServerProgress(syncServerProgress, cancellationToken);
    }

    public async Task FileMergeProgress(
        FileMergeProgress fileMergeProgress,
        CancellationToken cancellationToken = default
    )
    {
        _log.Debug(
            "Sending progress: {MessageTypesNotification} => {@FileMergeProgress}",
            MessageTypes.FileMergeProgress.ToString(),
            fileMergeProgress
        );
        await Clients.All.FileMergeProgress(fileMergeProgress, cancellationToken);
    }

    public async Task ServerConnectionCheckStatusProgress(
        ServerConnectionCheckStatusProgressDTO serverConnectionCheckStatusProgress,
        CancellationToken cancellationToken = default
    )
    {
        _log.Debug(
            "Sending progress: {MessageTypesNotification} => {@ServerConnectionCheckStatusProgress}",
            MessageTypes.ServerConnectionCheckStatusProgress.ToString(),
            serverConnectionCheckStatusProgress
        );
        await Clients.All.ServerConnectionCheckStatusProgress(serverConnectionCheckStatusProgress, cancellationToken);
    }

    public async Task ServerDownloadProgress(
        ServerDownloadProgressDTO serverDownloadProgress,
        CancellationToken cancellationToken = default
    )
    {
        _log.Debug(
            "Sending progress: {MessageTypesNotification} => {@ServerDownloadProgress}",
            MessageTypes.ServerDownloadProgress.ToString(),
            serverDownloadProgress
        );
        await Clients.All.ServerDownloadProgress(serverDownloadProgress, cancellationToken);
    }

    public async Task DownloadTaskUpdate(DownloadTaskDTO downloadTask, CancellationToken cancellationToken = default)
    {
        _log.Debug(
            "Sending progress: {MessageTypesNotification} => {@DownloadTaskUpdate}",
            MessageTypes.DownloadTaskUpdate.ToString(),
            downloadTask
        );
        await Clients.All.DownloadTaskUpdate(downloadTask, cancellationToken);
    }

    public async Task LibraryProgress(LibraryProgress libraryProgress, CancellationToken cancellationToken = default)
    {
        _log.Debug(
            "Sending progress: {MessageTypesNotification} => {@LibraryProgress}",
            MessageTypes.LibraryProgress.ToString(),
            libraryProgress
        );
        await Clients.All.LibraryProgress(libraryProgress, cancellationToken);
    }
}
