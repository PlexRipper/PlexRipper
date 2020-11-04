using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using PlexRipper.Application.Common;
using PlexRipper.Domain;
using PlexRipper.DownloadManager.Common;
using PlexRipper.SignalR.Common;
using PlexRipper.SignalR.Hubs;

namespace PlexRipper.SignalR
{
    /// <summary>
    /// A SignalR wrapper to send data to the front-end implementation.
    /// </summary>
    public class SignalRService : ISignalRService
    {
        private readonly IHubContext<ProgressHub> _progressHub;

        private readonly IHubContext<NotificationHub> _notificationHub;

        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalRService"/> class.
        /// </summary>
        /// <param name="progressHub">The <see cref="ProgressHub"/>.</param>
        /// <param name="notificationHub">The <see cref="NotificationHub"/>.</param>
        public SignalRService(IHubContext<ProgressHub> progressHub, IHubContext<NotificationHub> notificationHub, IMapper mapper)
        {
            _progressHub = progressHub;
            _notificationHub = notificationHub;
            _mapper = mapper;
        }

        #region ProgressHub

        public async Task SendLibraryProgressUpdate(int id, int received, int total, bool isRefreshing = true)
        {
            if (_progressHub?.Clients?.All == null)
            {
                return;
            }

            var progress = new LibraryProgress
            {
                Id = id,
                Received = received,
                Total = total,
                Percentage = DataFormat.GetPercentage(received, total),
                IsRefreshing = isRefreshing,
                IsComplete = received >= total,
            };

            await _progressHub.Clients.All.SendAsync(nameof(LibraryProgress), progress);
        }

        public async Task SendDownloadTaskCreationProgressUpdate(int plexLibraryId, int current, int total)
        {
            var progress = new DownloadTaskCreationProgress
            {
                PlexLibraryId = plexLibraryId,
                Percentage = DataFormat.GetPercentage(current, total),
                Current = current,
                Total = total,
                IsComplete = current >= total,
            };

            if (_progressHub?.Clients?.All == null)
            {
                return;
            }

            await _progressHub.Clients.All.SendAsync(nameof(DownloadTaskCreationProgress), progress);
        }

        /// <inheritdoc/>
        public async Task SendDownloadProgressUpdate(IDownloadProgress downloadProgress)
        {
            if (_progressHub?.Clients?.All == null)
            {
                return;
            }

            await _progressHub.Clients.All.SendAsync("DownloadProgress", downloadProgress);
        }

        /// <inheritdoc/>
        public async Task SendDownloadStatusUpdate(int id, DownloadStatus downloadStatus)
        {
            if (_progressHub?.Clients?.All == null)
            {
                return;
            }

            var downloadStatusChanged = new DownloadStatusChanged(id, downloadStatus);

            await _progressHub.Clients.All.SendAsync(nameof(DownloadStatusChanged), downloadStatusChanged);
        }

        /// <inheritdoc/>
        public void SendFileMergeProgressUpdate(FileMergeProgress fileMergeProgress)
        {
            if (_progressHub?.Clients?.All == null)
            {
                return;
            }

            Task.Run(() => _progressHub.Clients.All.SendAsync(nameof(FileMergeProgress), fileMergeProgress));
        }

        #endregion

        #region NotificationHub

        public async Task SendNotification(Notification notification)
        {
            if (_notificationHub?.Clients?.All == null)
            {
                return;
            }

            var notificationUpdate = _mapper.Map<NotificationUpdate>(notification);
            await _notificationHub.Clients.All.SendAsync(nameof(Notification), notificationUpdate);
        }

        #endregion
    }
}