using PlexRipper.Domain.Entities;
using System.Threading.Tasks;

namespace PlexRipper.Application.Common.Interfaces.DownloadManager
{
    public interface IDownloadManager
    {
        int ActiveDownloads { get; }
        int CompletedDownloads { get; }
        int TotalDownloads { get; }
        Task StartDownloadAsync(DownloadTask downloadTask);
    }
}
