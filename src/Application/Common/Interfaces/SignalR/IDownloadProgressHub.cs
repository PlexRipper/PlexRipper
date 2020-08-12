using System.Threading.Tasks;
using PlexRipper.Domain.Types;

namespace PlexRipper.Application.Common.Interfaces.SignalR
{
    public interface IDownloadProgressHub
    {
        Task SendMessageAsync(string user, string message);
        Task SendDownloadProgressAsync(DownloadProgress downloadProgress);
    }
}
