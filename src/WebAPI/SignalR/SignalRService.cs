using AutoMapper;
using BackgroundServices.Contracts;
using Logging.Interface;
using Microsoft.AspNetCore.SignalR;
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
        ILog log,
        IHubContext<ProgressHub, IProgressHub> progressHub,
        IHubContext<NotificationHub, INotificationHub> notificationHub,
        IMapper mapper)
    {
        _log = log;
        _progressHub = progressHub;
        _notificationHub = notificationHub;
        _mapper = mapper;
    }

    #region ProgressHub

    public async Task SendLibraryProgressUpdateAsync(int id, int received, int total, bool isRefreshing = true)
    {
        var libraryProgress = new LibraryProgress(id, received, total, isRefreshing);
        await _progressHub.Clients.All.LibraryProgress(libraryProgress);
    }

    #region DownloadProgress

    public async Task SendDownloadProgressUpdateAsync(int plexServerId, List<DownloadTaskGeneric> downloadTasks, CancellationToken cancellationToken = default)
    {
        var update = _mapper.Map<List<ServerDownloadProgressDTO>>(downloadTasks);
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
        var progressDTO = _mapper.Map<InspectServerProgressDTO>(progress);
        await _progressHub.Clients.All.InspectServerProgress(progressDTO);
    }

    public async Task SendServerConnectionCheckStatusProgressAsync(ServerConnectionCheckStatusProgress progress)
    {
        var progressDTO = _mapper.Map<ServerConnectionCheckStatusProgressDTO>(progress);
        await _progressHub.Clients.All.ServerConnectionCheckStatusProgress(progressDTO);
    }

    /// <inheritdoc/>
    public async Task SendFileMergeProgressUpdateAsync(FileMergeProgress fileMergeProgress, CancellationToken cancellationToken = default)
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
        var notificationDto = _mapper.Map<NotificationDTO>(notification);
        await _notificationHub.Clients.All.Notification(notificationDto);
    }

    #endregion

    #region JobStateNotification

    public async Task SendJobStatusUpdateAsync(JobStatusUpdate jobStatusUpdate)
    {
        var jobStatusUpdateDto = _mapper.Map<JobStatusUpdateDTO>(jobStatusUpdate);
        await _progressHub.Clients.All.JobStatusUpdate(jobStatusUpdateDto);
    }

    #endregion
}