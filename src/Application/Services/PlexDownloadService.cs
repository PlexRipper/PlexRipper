using PlexRipper.Application.Common.Interfaces;
using PlexRipper.Application.Common.Interfaces.DownloadManager;
using PlexRipper.Domain.Entities;
using PlexRipper.Domain.Enums;
using Serilog;
using System.Threading.Tasks;

namespace PlexRipper.Application.Services
{
    public class PlexDownloadService : IPlexDownloadService
    {

        private readonly IDownloadManager _downloadManager;
        private readonly IPlexAuthenticationService _plexAuthenticationService;
        private readonly IPlexApiService _plexApiService;

        public PlexDownloadService(IDownloadManager downloadManager, IPlexAuthenticationService plexAuthenticationService, IPlexApiService plexApiService)
        {
            _downloadManager = downloadManager;
            _plexAuthenticationService = plexAuthenticationService;
            _plexApiService = plexApiService;
        }

        public void StartDownload(DownloadTask downloadTask)
        {
            _downloadManager.StartDownloadAsync(downloadTask);
        }

        public Task<string> GetPlexTokenAsync(PlexAccount plexAccount)
        {
            return _plexAuthenticationService.GetPlexToken(plexAccount);
        }

        public async Task<DownloadTask> GetDownloadRequestAsync(PlexMovie movie)
        {
            Log.Debug($"Creating download request for movie {movie.Title}");
            var server = movie.PlexLibrary.PlexServer;
            var metaData = await _plexApiService.GetMediaMetaDataAsync(server.AccessToken, server.BaseUrl, movie.RatingKey);


            return metaData != null ? new DownloadTask
            {
                PlexServerId = server.Id,
                FolderPathId = 1, // TODO make this dynamic
                FileLocationUrl = metaData.ObfuscatedFilePath,
                Status = DownloadStatus.Initialized,
                FileName = metaData.FileName
            } : null;
        }
    }
}
