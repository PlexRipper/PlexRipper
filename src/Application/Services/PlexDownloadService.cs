using PlexRipper.Application.Common.Interfaces;
using PlexRipper.Application.Common.Interfaces.DownloadManager;
using PlexRipper.Application.Common.Models;
using PlexRipper.Domain.Entities;
using System.Threading.Tasks;

namespace PlexRipper.Application.Services
{
    public class PlexDownloadService : IPlexDownloadService
    {
        private readonly IDownloadManager _downloadManager;
        private readonly IPlexAuthenticationService _plexAuthenticationService;

        public PlexDownloadService(IDownloadManager downloadManager, IPlexAuthenticationService plexAuthenticationService)
        {
            _downloadManager = downloadManager;
            _plexAuthenticationService = plexAuthenticationService;
        }

        public void StartDownload(DownloadRequest downloadRequest)
        {
            _downloadManager.StartDownload(downloadRequest);
        }

        public Task<string> GetPlexTokenAsync(PlexAccount plexAccount)
        {
            return _plexAuthenticationService.GetPlexToken(plexAccount);
        }
    }
}
