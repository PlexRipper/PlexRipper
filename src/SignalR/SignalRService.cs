using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using PlexRipper.Application.Common.Interfaces.SignalR;
using PlexRipper.Domain.Common;
using PlexRipper.Domain.Types;
using PlexRipper.SignalR.Common;
using PlexRipper.SignalR.Hubs;

namespace PlexRipper.SignalR
{
    public class SignalRService : ISignalRService
    {
        private readonly IHubContext<LibraryProgressHub> _libraryProgress;
        private readonly IHubContext<DownloadHub> _downloadHub;

        public SignalRService(IHubContext<LibraryProgressHub> libraryProgress, IHubContext<DownloadHub> downloadHub)
        {
            _libraryProgress = libraryProgress;
            _downloadHub = downloadHub;
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
                IsComplete = received >= total
            };

            await _libraryProgress.Clients.All.SendAsync("LibraryProgress", progress);
        }

        public async Task SendDownloadTaskCreationProgressUpdate(int plexLibraryId, int current, int total)
        {
            var progress = new DownloadTaskCreationProgress
            {
                PlexLibraryId = plexLibraryId,
                Percentage = DataFormat.GetPercentage(current, total),
                Current = current,
                Total = total,
                IsComplete = current >= total
            };

            await _downloadHub.Clients.All.SendAsync("DownloadTaskCreation", progress);
        }


    }
}