using AutoMapper;
using BackgroundServices.Contracts;
using Logging.Interface;
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
    private readonly ILog _log;
    private readonly IHubContext<ProgressHub> _progressHub;

    private readonly IHubContext<NotificationHub> _notificationHub;

    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="SignalRService"/> class.
    /// </summary>
    /// <param name="progressHub">The <see cref="ProgressHub"/>.</param>
    /// <param name="notificationHub">The <see cref="NotificationHub"/>.</param>
    /// <param name="mapper"></param>
    public SignalRService(ILog log, IHubContext<ProgressHub> progressHub, IHubContext<NotificationHub> notificationHub, IMapper mapper)
    {
        _log = log;
        _progressHub = progressHub;
        _notificationHub = notificationHub;
        _mapper = mapper;
    }

    #region ProgressHub

    public void SendLibraryProgressUpdate(LibraryProgress libraryProgress)
    {
        if (_progressHub?.Clients?.All == null)
        {
            _log.WarningLine("No Clients connected to ProgressHub");
            return;
        }

        Task.Run(() => _progressHub.Clients.All.SendAsync(MessageTypes.LibraryProgress.ToString(), libraryProgress));
    }

    public void SendLibraryProgressUpdate(int id, int received, int total, bool isRefreshing = true)
    {
        if (_progressHub?.Clients?.All == null)
        {
            _log.WarningLine("No Clients connected to ProgressHub");
            return;
        }

        SendLibraryProgressUpdate(new LibraryProgress(id, received, total, isRefreshing));
    }

    public async Task SendDownloadTaskCreationProgressUpdate(int current, int total)
    {
        if (_progressHub?.Clients?.All == null)
        {
            _log.WarningLine("No Clients connected to ProgressHub");
            return;
        }

        var progress = new DownloadTaskCreationProgress
        {
            Percentage = DataFormat.GetPercentage(current, total),
            Current = current,
            Total = total,
            IsComplete = current >= total,
        };

        await _progressHub.Clients.All.SendAsync(MessageTypes.DownloadTaskCreationProgress.ToString(), progress);
    }

    /// <inheritdoc/>
    public void SendDownloadTaskUpdate(DownloadTask downloadTask)
    {
        if (_progressHub?.Clients?.All == null)
        {
            _log.WarningLine("No Clients connected to ProgressHub");
            return;
        }

        var downloadTaskDTO = _mapper.Map<DownloadTaskDTO>(downloadTask);
        Task.Run(() => _progressHub.Clients.All.SendAsync(MessageTypes.DownloadTaskUpdate.ToString(), downloadTaskDTO));
    }

    #region DownloadProgress

    public async Task SendDownloadProgressUpdate(int plexServerId, List<DownloadTask> downloadTasks)
    {
        if (_progressHub?.Clients?.All == null)
        {
            _log.WarningLine("No Clients connected to ProgressHub");
            return;
        }

        var downloadTasksDTO = _mapper.Map<List<DownloadProgressDTO>>(downloadTasks);
        var update = new ServerDownloadProgressDTO
        {
            Id = plexServerId,
            Downloads = downloadTasksDTO,
        };

        await _progressHub.Clients.All.SendAsync(MessageTypes.ServerDownloadProgress.ToString(), update);
    }

    #endregion

    public async Task SendServerInspectStatusProgress(InspectServerProgress progress)
    {
        if (_progressHub?.Clients?.All == null)
        {
            _log.WarningLine("No Clients connected to ProgressHub");
            return;
        }

        _log.Debug("{MessageTypesInspectServerProgress} => {@Progress}", MessageTypes.InspectServerProgress, progress);
        var progressDTO = _mapper.Map<InspectServerProgressDTO>(progress);
        await _progressHub.Clients.All.SendAsync(MessageTypes.InspectServerProgress.ToString(), progressDTO);
    }

    public async Task SendServerConnectionCheckStatusProgress(ServerConnectionCheckStatusProgress progress)
    {
        if (_progressHub?.Clients?.All == null)
        {
            _log.WarningLine("No Clients connected to ProgressHub");
            return;
        }

        _log.Debug("Sending progress: {MessageTypesServerConnectionCheckStatusProgress} => {Progress}", MessageTypes.ServerConnectionCheckStatusProgress,
            progress);

        var progressDTO = _mapper.Map<ServerConnectionCheckStatusProgressDTO>(progress);
        await _progressHub.Clients.All.SendAsync(MessageTypes.ServerConnectionCheckStatusProgress.ToString(), progressDTO);
    }

    /// <inheritdoc/>
    public async Task SendFileMergeProgressUpdate(FileMergeProgress fileMergeProgress)
    {
        if (_progressHub?.Clients?.All == null)
        {
            _log.WarningLine("No Clients connected to ProgressHub");
            return;
        }

        await _progressHub.Clients.All.SendAsync(MessageTypes.FileMergeProgress.ToString(), fileMergeProgress);
    }

    public void SendServerSyncProgressUpdate(SyncServerProgress syncServerProgress)
    {
        if (_progressHub?.Clients?.All == null)
        {
            _log.WarningLine("No Clients connected to ProgressHub");
            return;
        }

        Task.Run(() => _progressHub.Clients.All.SendAsync(MessageTypes.SyncServerProgress.ToString(), syncServerProgress));
    }

    #endregion

    #region NotificationHub

    public async Task SendNotification(Notification notification)
    {
        if (_notificationHub?.Clients?.All == null)
        {
            _log.WarningLine("No Clients connected to NotificationHub");
            return;
        }

        var notificationDto = _mapper.Map<NotificationDTO>(notification);
        _log.Debug("Sending notification: {MessageTypesNotification} => {@NotificationDto}", MessageTypes.Notification, notificationDto);
        await _notificationHub.Clients.All.SendAsync(MessageTypes.Notification.ToString(), notificationDto);
    }

    #endregion

    #region JobStateNotification

    public async Task SendJobStatusUpdate(JobStatusUpdate jobStatusUpdate)
    {
        if (_notificationHub?.Clients?.All == null)
        {
            _log.WarningLine("No Clients connected to NotificationHub");
            return;
        }

        var jobStatusUpdateDto = _mapper.Map<JobStatusUpdateDTO>(jobStatusUpdate);
        _log.Debug("Sending update: {MessageTypesJobStatusUpdate} => {@JobStatusUpdateDto}", MessageTypes.JobStatusUpdate, jobStatusUpdateDto);
        await _progressHub.Clients.All.SendAsync(MessageTypes.JobStatusUpdate.ToString(), jobStatusUpdateDto);
    }

    #endregion
}