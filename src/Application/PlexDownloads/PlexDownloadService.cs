using FluentResults;
using MediatR;
using PlexRipper.Application.Common.Interfaces;
using PlexRipper.Application.Common.Interfaces.DownloadManager;
using PlexRipper.Application.Common.Interfaces.PlexApi;
using PlexRipper.Application.PlexMovies.Queries;
using PlexRipper.Domain;
using PlexRipper.Domain.Entities;
using PlexRipper.Domain.Enums;
using System.Threading.Tasks;

namespace PlexRipper.Application.PlexDownloads
{
    public class PlexDownloadService : IPlexDownloadService
    {
        private readonly IMediator _mediator;
        private readonly IDownloadManager _downloadManager;
        private readonly IPlexAuthenticationService _plexAuthenticationService;
        private readonly IPlexApiService _plexApiService;

        public PlexDownloadService(IMediator mediator, IDownloadManager downloadManager, IPlexAuthenticationService plexAuthenticationService, IPlexApiService plexApiService)
        {
            _mediator = mediator;
            _downloadManager = downloadManager;
            _plexAuthenticationService = plexAuthenticationService;
            _plexApiService = plexApiService;
        }

        public Task StartDownloadAsync(DownloadTask downloadTask)
        {
            return _downloadManager.StartDownloadAsync(downloadTask);
        }

        public Task<string> GetPlexTokenAsync(PlexAccount plexAccount)
        {
            return _plexAuthenticationService.GetPlexTokenAsync(plexAccount);
        }

        public async Task<Result<DownloadTask>> GetDownloadRequestAsync(int plexAccountId, PlexMovie plexMovie)
        {
            if (plexMovie == null)
            {
                return Result.Fail("plexMovie parameter is null");
            }

            Log.Debug($"Creating download request for movie {plexMovie.Title}");

            var server = plexMovie.PlexLibrary.PlexServer;
            var token = await _plexAuthenticationService.GetPlexServerTokenAsync(plexAccountId, server.Id);

            if (token.IsFailed)
            {
                return token.ToResult<DownloadTask>();
            }

            var metaData = await _plexApiService.GetMediaMetaDataAsync(token.Value, server.BaseUrl, plexMovie.RatingKey);

            if (metaData != null)
            {
                return Result.Ok(new DownloadTask
                {
                    PlexServerId = server.Id,
                    FolderPathId = 1, // TODO make this dynamic
                    FileLocationUrl = metaData.ObfuscatedFilePath,
                    Status = DownloadStatus.Initialized,
                    FileName = metaData.FileName,
                    PlexServerAuthToken = token.Value
                });
            }

            return Result.Fail($"Failed to retrieve metadata for plex movie with id: {plexMovie.Id}");
        }

        public async Task<Result<bool>> DownloadMovieAsync(int plexAccountId, int plexMovieId)
        {
            var plexMovie = await _mediator.Send(new GetPlexMovieByIdQuery(plexMovieId));
            if (plexMovie.IsFailed)
            {
                return plexMovie.ToResult<bool>();
            }

            var downloadTask = await GetDownloadRequestAsync(plexAccountId, plexMovie.Value);
            if (downloadTask.IsFailed)
            {
                return downloadTask.ToResult<bool>();
            }

            await StartDownloadAsync(downloadTask.Value);
            return Result.Ok(true);
        }
    }
}
