using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using PlexRipper.Application.Common.Interfaces.SignalR;
using PlexRipper.Domain.Common;
using PlexRipper.Domain.Types;
using PlexRipper.SignalR.Hubs;

namespace PlexRipper.SignalR
{
    public class SignalRService : ISignalRService
    {
        private readonly IHubContext<LibraryProgressHub> _libraryProgress;

        public SignalRService(IHubContext<LibraryProgressHub> libraryProgress)
        {
            _libraryProgress = libraryProgress;
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
    }
}