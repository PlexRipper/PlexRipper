using Microsoft.AspNetCore.SignalR;
using PlexRipper.Domain;
using PlexRipper.Domain.Types;
using System.Threading.Tasks;

namespace PlexRipper.SignalR.Hubs
{
    public class LibraryProgressHub : Hub
    {
        public Task SendDownloadProgressAsync(LibraryProgress libraryProgress)
        {
            if (Clients != null)
            {
                return Clients.All.SendAsync("LibraryProgress", libraryProgress);
            }
            Log.Error("Clients is null, make sure a client has been subscribed!");
            return new Task(() => { });
        }
    }
}
