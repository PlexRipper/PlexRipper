using PlexRipper.Domain.Entities;
using System.Threading.Tasks;
using FluentResults;

namespace PlexRipper.Application.Common.Interfaces.DownloadManager
{
    public interface IDownloadManager
    {
        int ActiveDownloads { get; }
        int CompletedDownloads { get; }
        int TotalDownloads { get; }
        Task StartDownloadAsync(DownloadTask downloadTask);
        Result<bool> CancelDownload(int downloadTaskId);
    }
}
