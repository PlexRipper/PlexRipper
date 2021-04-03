using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using PlexRipper.Application.Common;
using PlexRipper.Application.Common.DTO.DownloadManager;
using PlexRipper.Domain;
using PlexRipper.WebAPI.Common.DTO;
using PlexRipper.WebAPI.SignalR.Common;
using PlexRipper.WebAPI.SignalR.Hubs;

namespace PlexRipper.WebAPI.SignalR
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

        public async Task SendPlexAccountRefreshUpdate(int plexAccountId, int received, int total, bool isRefreshing = true)
        {
            if (_progressHub?.Clients?.All == null)
            {
                Log.Warning("No Clients connected to ProgressHub");
                return;
            }

            var progress = new PlexAccountRefreshProgress
            {
                PlexAccountId = plexAccountId,
                Received = received,
                Total = total,
                Percentage = DataFormat.GetPercentage(received, total),
                IsRefreshing = isRefreshing,
                IsComplete = received >= total,
            };

            await _progressHub.Clients.All.SendAsync(nameof(PlexAccountRefreshProgress), progress);
        }

        public async Task SendLibraryProgressUpdate(int id, int received, int total, bool isRefreshing = true)
        {
            if (_progressHub?.Clients?.All == null)
            {
                Log.Warning("No Clients connected to ProgressHub");
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

        public async Task SendDownloadTaskCreationProgressUpdate(int current, int total)
        {
            if (_progressHub?.Clients?.All == null)
            {
                Log.Warning("No Clients connected to ProgressHub");
                return;
            }

            var progress = new DownloadTaskCreationProgress
            {
                Percentage = DataFormat.GetPercentage(current, total),
                Current = current,
                Total = total,
                IsComplete = current >= total,
            };

            await _progressHub.Clients.All.SendAsync(nameof(DownloadTaskCreationProgress), progress);
        }

        /// <inheritdoc/>
        public async Task SendDownloadTaskUpdate(DownloadClientUpdate downloadClientUpdate)
        {
            if (_progressHub?.Clients?.All == null)
            {
                Log.Warning("No Clients connected to ProgressHub");
                return;
            }

            var downloadTaskDTO = _mapper.Map<DownloadTaskDTO>(downloadClientUpdate);
            await _progressHub.Clients.All.SendAsync("DownloadTaskUpdate", downloadTaskDTO);
        }

        /// <inheritdoc/>
        public void SendFileMergeProgressUpdate(FileMergeProgress fileMergeProgress)
        {
            if (_progressHub?.Clients?.All == null)
            {
                Log.Warning("No Clients connected to ProgressHub");
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
                Log.Warning("No Clients connected to NotificationHub");
                return;
            }

            var notificationUpdate = _mapper.Map<NotificationDTO>(notification);
            await _notificationHub.Clients.All.SendAsync(nameof(Notification), notificationUpdate);
        }

        #endregion
    }
}