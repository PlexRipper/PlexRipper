using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace PlexRipper.WebAPI.Hubs
{
    public class DownloadProgressHub : Hub
    {
        public Task SendMessage(string user, string message)
        {
            return Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}
