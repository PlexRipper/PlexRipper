using Microsoft.AspNetCore.SignalR;
using PlexRipper.Application.Common.Interfaces.WebApi;
using PlexRipper.Domain;
using PlexRipper.Domain.Types;
using System.Threading.Tasks;

namespace PlexRipper.SignalR.Hubs
{
    public class DownloadProgressHub : Hub, IDownloadProgressHub
    {
        public Task SendMessageAsync(string user, string message)
        {
            return Clients?.All.SendAsync("ReceiveMessage", user, message);
        }

        public Task SendDownloadProgressAsync(DownloadProgress downloadProgress)
        {
            if (Clients != null)
            {
                return Clients.All.SendAsync("DownloadProgress", downloadProgress);
            }
            Log.Error("Clients is null, make sure a client has been subscribed!");
            return new Task(() => { });
        }
    }
}
