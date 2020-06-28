using PlexRipper.Application.Common.Interfaces;
using PlexRipper.Application.Common.Interfaces.DownloadManager;
using PlexRipper.Application.Common.Models;
using PlexRipper.Domain.Entities;
using Serilog;
using System.Threading.Tasks;

namespace PlexRipper.Application.Services
{
    public class PlexDownloadService : IPlexDownloadService
    {
        public ILogger Log { get; }
        private readonly IDownloadManager _downloadManager;
        private readonly IPlexAuthenticationService _plexAuthenticationService;
        private readonly IPlexApiService _plexApiService;

        public PlexDownloadService(IDownloadManager downloadManager, IPlexAuthenticationService plexAuthenticationService, IPlexApiService plexApiService, ILogger log)
        {
            Log = log;
            _downloadManager = downloadManager;
            _plexAuthenticationService = plexAuthenticationService;
            _plexApiService = plexApiService;
        }

        public void StartDownload(DownloadRequest downloadRequest)
        {
            _downloadManager.StartDownload(downloadRequest);
        }

        public Task<string> GetPlexTokenAsync(PlexAccount plexAccount)
        {
            return _plexAuthenticationService.GetPlexToken(plexAccount);
        }

        public async Task<DownloadRequest> GetDownloadRequestAsync(PlexMovie movie)
        {
            Log.Debug($"Creating download request for movie {movie.Title}");
            var server = movie.PlexLibrary.PlexServer;
            var metaData = await _plexApiService.GetMediaMetaDataAsync(server.AccessToken, server.BaseUrl, movie.RatingKey);
            return metaData != null ? new DownloadRequest($"{server.BaseUrl}{metaData.ObfuscatedFilePath}", server.AccessToken, metaData.FileName) : null;
        }
    }
}
