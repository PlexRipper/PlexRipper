using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using PlexRipper.Application.Common;
using PlexRipper.Domain;
using PlexRipper.Domain.Types;
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
        private readonly ProgressHub _progressHub;

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalRService"/> class.
        /// </summary>
        /// <param name="progressHub">The <see cref="ProgressHub"/>.</param>
        public SignalRService(ProgressHub progressHub)
        {
            _progressHub = progressHub;
        }

        public async Task SendLibraryProgressUpdate(int id, int received, int total, bool isRefreshing = true)
        {
            var progress = new LibraryProgress
            {
                Id = id,
                Received = received,
                Total = total,
                Percentage = DataFormat.GetPercentage(received, total),
                IsRefreshing = isRefreshing,
                IsComplete = received >= total,
            };

            if (_progressHub?.Clients?.All == null)
            {
                return;
            }

            await _progressHub.Clients.All.SendAsync("LibraryProgress", progress);
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

            await _progressHub.Clients.All.SendAsync("DownloadTaskCreation", progress);
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

            await _progressHub.Clients.All.SendAsync("DownloadStatus", downloadStatusChanged);
        }

        /// <inheritdoc/>
        public void SendFileMergeProgressUpdate(FileMergeProgress fileMergeProgress)
        {
            if (_progressHub?.Clients?.All == null)
            {
                return;
            }

            Task.Run(() => _progressHub.Clients.All.SendAsync("FileMergeProgress", fileMergeProgress));
        }
    }
}