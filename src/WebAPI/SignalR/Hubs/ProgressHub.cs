using Application.Contracts;
using Logging.Interface;
using Microsoft.AspNetCore.SignalR;
using PlexRipper.WebAPI.SignalR.Common;
using SignalRSwaggerGen.Attributes;
using WebAPI.Contracts;

namespace PlexRipper.WebAPI.SignalR.Hubs;

public class ProgressHub : Hub<IProgressHub>
{
    private readonly ILog<ProgressHub> _log;

    public ProgressHub(ILog<ProgressHub> log)
    {
        _log = log;
    }

    public async Task JobStatusUpdate(
        [SignalRParam(paramType: typeof(JobStatusUpdateDTO))]
        JobStatusUpdateDTO jobStatusUpdate,
        [SignalRHidden] CancellationToken cancellationToken = default)
    {
        _log.Debug("Sending progress: {MessageTypesNotification} => {@JobStatusUpdateDto}",
            MessageTypes.JobStatusUpdate.ToString(), jobStatusUpdate);
        await Clients.All.JobStatusUpdate(jobStatusUpdate, cancellationToken);
    }

    public async Task SyncServerProgress(
        [SignalRParam(paramType: typeof(SyncServerProgress))]
        SyncServerProgress SyncServerProgress,
        [SignalRHidden] CancellationToken cancellationToken = default)
    {
        _log.Debug("Sending progress: {MessageTypesNotification} => {@SyncServerProgress}",
            MessageTypes.SyncServerProgress.ToString(), SyncServerProgress);
        await Clients.All.SyncServerProgress(SyncServerProgress, cancellationToken);
    }

    public async Task FileMergeProgress(
        [SignalRParam(paramType: typeof(FileMergeProgress))]
        FileMergeProgress fileMergeProgress,
        [SignalRHidden] CancellationToken cancellationToken = default)
    {
        _log.Debug("Sending progress: {MessageTypesNotification} => {@FileMergeProgress}",
            MessageTypes.FileMergeProgress.ToString(), fileMergeProgress);
        await Clients.All.FileMergeProgress(fileMergeProgress, cancellationToken);
    }

    public async Task ServerConnectionCheckStatusProgress(
        [SignalRParam(paramType: typeof(ServerConnectionCheckStatusProgressDTO))]
        ServerConnectionCheckStatusProgressDTO serverConnectionCheckStatusProgress,
        [SignalRHidden] CancellationToken cancellationToken = default)
    {
        _log.Debug("Sending progress: {MessageTypesNotification} => {@ServerConnectionCheckStatusProgress}",
            MessageTypes.ServerConnectionCheckStatusProgress.ToString(), serverConnectionCheckStatusProgress);
        await Clients.All.ServerConnectionCheckStatusProgress(serverConnectionCheckStatusProgress, cancellationToken);
    }

    public async Task ServerDownloadProgress(
        [SignalRParam(paramType: typeof(ServerDownloadProgressDTO))]
        ServerDownloadProgressDTO serverDownloadProgress,
        [SignalRHidden] CancellationToken cancellationToken = default)
    {
        _log.Debug("Sending progress: {MessageTypesNotification} => {@ServerDownloadProgress}",
            MessageTypes.ServerDownloadProgress.ToString(), serverDownloadProgress);
        await Clients.All.ServerDownloadProgress(serverDownloadProgress, cancellationToken);
    }

    public async Task DownloadTaskUpdate(
        [SignalRParam(paramType: typeof(DownloadTaskDTO))]
        DownloadTaskDTO downloadTask,
        [SignalRHidden] CancellationToken cancellationToken = default)
    {
        _log.Debug("Sending progress: {MessageTypesNotification} => {@DownloadTaskUpdate}",
            MessageTypes.DownloadTaskUpdate.ToString(), downloadTask);
        await Clients.All.DownloadTaskUpdate(downloadTask, cancellationToken);
    }

    public async Task LibraryProgress(
        [SignalRParam(paramType: typeof(LibraryProgress))]
        LibraryProgress libraryProgress,
        [SignalRHidden] CancellationToken cancellationToken = default)
    {
        _log.Debug("Sending progress: {MessageTypesNotification} => {@LibraryProgress}",
            MessageTypes.LibraryProgress.ToString(), libraryProgress);
        await Clients.All.LibraryProgress(libraryProgress, cancellationToken);
    }
}