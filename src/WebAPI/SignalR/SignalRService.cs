using AutoMapper;
using BackgroundServices.Contracts;
using Microsoft.AspNetCore.SignalR;
using PlexRipper.DownloadManager;
using PlexRipper.WebAPI.Common.DTO;
using PlexRipper.WebAPI.SignalR.Common;
using PlexRipper.WebAPI.SignalR.Hubs;
using WebAPI.Contracts;

namespace PlexRipper.WebAPI.SignalR;

/// <summary>
/// A SignalR wrapper to send data to the front-end implementation.
/// </summary>
public class SignalRService : ISignalRService
{
    private readonly IHubContext<ProgressHub, IProgressHub> _progressHub;

    private readonly IHubContext<NotificationHub, INotificationHub> _notificationHub;

    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="SignalRService"/> class.
    /// </summary>
    /// <param name="progressHub">The <see cref="ProgressHub"/>.</param>
    /// <param name="notificationHub">The <see cref="NotificationHub"/>.</param>
    /// <param name="mapper"></param>
    public SignalRService(
        IHubContext<ProgressHub, IProgressHub> progressHub,
        IHubContext<NotificationHub, INotificationHub> notificationHub,
        IMapper mapper)
    {
        _progressHub = progressHub;
        _notificationHub = notificationHub;
        _mapper = mapper;
    }

    #region ProgressHub

    public async Task SendLibraryProgressUpdate(int id, int received, int total, bool isRefreshing = true)
    {
        var libraryProgress = new LibraryProgress(id, received, total, isRefreshing);
        await _progressHub.Clients.All.LibraryProgress(libraryProgress);
    }

    /// <inheritdoc/>
    public void SendDownloadTaskUpdate(DownloadTask downloadTask)
    {
        var downloadTaskDTO = _mapper.Map<DownloadTaskDTO>(downloadTask);
        _progressHub.Clients.All.DownloadTaskUpdate(downloadTaskDTO);
    }

    #region DownloadProgress

    public async Task SendDownloadProgressUpdate(int plexServerId, List<DownloadTask> downloadTasks)
    {
        var downloadTasksDTO = _mapper.Map<List<DownloadProgressDTO>>(downloadTasks);
        var update = new ServerDownloadProgressDTO
        {
            Id = plexServerId,
            Downloads = downloadTasksDTO,
        };

        await _progressHub.Clients.All.ServerDownloadProgress(update);
    }

    #endregion

    public async Task SendServerInspectStatusProgress(InspectServerProgress progress)
    {
        var progressDTO = _mapper.Map<InspectServerProgressDTO>(progress);
        await _progressHub.Clients.All.InspectServerProgress(progressDTO);
    }

    public async Task SendServerConnectionCheckStatusProgress(ServerConnectionCheckStatusProgress progress)
    {
        var progressDTO = _mapper.Map<ServerConnectionCheckStatusProgressDTO>(progress);
        await _progressHub.Clients.All.ServerConnectionCheckStatusProgress(progressDTO);
    }

    /// <inheritdoc/>
    public async Task SendFileMergeProgressUpdate(FileMergeProgress fileMergeProgress)
    {
        await _progressHub.Clients.All.FileMergeProgress(fileMergeProgress);
    }

    public async Task SendServerSyncProgressUpdate(SyncServerProgress syncServerProgress)
    {
        await _progressHub.Clients.All.SyncServerProgress(syncServerProgress);
    }

    #endregion

    #region NotificationHub

    public async Task SendNotification(Notification notification)
    {
        var notificationDto = _mapper.Map<NotificationDTO>(notification);

        // _log.Debug("Sending notification: {MessageTypesNotification} => {@NotificationDto}", MessageTypes.Notification, notificationDto);
        await _notificationHub.Clients.All.Notification(notificationDto);
    }

    #endregion

    #region JobStateNotification

    public async Task SendJobStatusUpdate(JobStatusUpdate jobStatusUpdate)
    {
        var jobStatusUpdateDto = _mapper.Map<JobStatusUpdateDTO>(jobStatusUpdate);
        await _progressHub.Clients.All.JobStatusUpdate(jobStatusUpdateDto);
    }

    #endregion
}