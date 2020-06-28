using PlexRipper.Domain.Entities;
using System.Threading.Tasks;

namespace PlexRipper.Application.Common.Interfaces
{
    public interface IPlexDownloadService
    {
        void StartDownload(DownloadTask downloadTask);
        Task<string> GetPlexTokenAsync(PlexAccount plexAccount);
        Task<DownloadTask> GetDownloadRequestAsync(PlexMovie movie);
    }
}
