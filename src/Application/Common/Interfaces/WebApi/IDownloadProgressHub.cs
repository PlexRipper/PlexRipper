using PlexRipper.Domain.Types;
using System.Threading.Tasks;

namespace PlexRipper.Application.Common.Interfaces.WebApi
{
    public interface IDownloadProgressHub
    {
        Task SendMessageAsync(string user, string message);
        Task SendDownloadProgressAsync(DownloadProgress downloadProgress);
    }
}
