using PlexRipper.Application.Common.Models;
using PlexRipper.Domain.Entities;
using System.Threading.Tasks;

namespace PlexRipper.Application.Common.Interfaces
{
    public interface IPlexDownloadService
    {
        void StartDownload(DownloadRequest downloadRequest);
        Task<string> GetPlexTokenAsync(PlexAccount plexAccount);
    }
}
