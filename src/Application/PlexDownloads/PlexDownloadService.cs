using FluentResults;
using MediatR;
using PlexRipper.Application.Common.Interfaces;
using PlexRipper.Application.Common.Interfaces.DownloadManager;
using PlexRipper.Application.Common.Interfaces.FileSystem;
using PlexRipper.Application.Common.Interfaces.PlexApi;
using PlexRipper.Application.FolderPaths.Queries;
using PlexRipper.Application.PlexDownloads.Commands;
using PlexRipper.Application.PlexDownloads.Queries;
using PlexRipper.Application.PlexMovies;
using PlexRipper.Domain;
using PlexRipper.Domain.Entities;
using PlexRipper.Domain.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;
using PlexRipper.Application.PlexServers.Queries;
using PlexRipper.Application.PlexTvShows.Queries;

namespace PlexRipper.Application.PlexDownloads
{
    public class PlexDownloadService : IPlexDownloadService
    {
        private readonly IMediator _mediator;
        private readonly IDownloadManager _downloadManager;
        private readonly IPlexAuthenticationService _plexAuthenticationService;
        private readonly IFileSystem _fileSystem;
        private readonly IPlexApiService _plexApiService;

        public PlexDownloadService(IMediator mediator, IDownloadManager downloadManager,
            IPlexAuthenticationService plexAuthenticationService, IFileSystem fileSystem,
            IPlexApiService plexApiService)
        {
            _mediator = mediator;
            _downloadManager = downloadManager;
            _plexAuthenticationService = plexAuthenticationService;
            _fileSystem = fileSystem;
            _plexApiService = plexApiService;
        }

        public Task<Result> StartDownloadAsync(DownloadTask downloadTask)
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

            if (plexMovie.PlexLibraryId <= 0)
            {
                return Result.Fail(
                    $"The plexMovie parameter should contain a valid PlexLibraryId, is: {plexMovie.PlexLibraryId}");
            }

            Log.Debug($"Creating download request for Movie: {plexMovie.Title}");

            var server = await _mediator.Send(new GetPlexServerByPlexLibraryIdQuery(plexMovie.PlexLibraryId));

            if (server.IsFailed)
            {
                return server.ToResult();
            }
            return await CreateDownloadTaskAsync(server.Value, plexAccountId, plexMovie.RatingKey);
        }

        public async Task<Result<DownloadTask>> GetDownloadRequestAsync(int plexAccountId, PlexTvShow plexTvShow)
        {
            if (plexTvShow == null)
            {
                return Result.Fail("plexTvShow parameter is null");
            }

            if (plexTvShow.PlexLibraryId <= 0)
            {
                return Result.Fail(
                    $"The plexTvShow parameter should contain a valid PlexLibraryId, is: {plexTvShow.PlexLibraryId}");
            }

            Log.Debug($"Creating download request for TvShow: {plexTvShow.Title}");

            var server = await _mediator.Send(new GetPlexServerByPlexLibraryIdQuery(plexTvShow.PlexLibraryId));
            if (server.IsFailed)
            {
                return server.ToResult();
            }
            return await CreateDownloadTaskAsync(server.Value, plexAccountId, plexTvShow.RatingKey);
        }

        /// <summary>
        /// Creates a <see cref="DownloadTask"/> which can be send to the <see cref="PlexDownloadService"/> to start a download of PlexMedia.
        /// </summary>
        /// <param name="server">The <see cref="PlexServer"/> to download from.</param>
        /// <param name="plexAccountId">The <see cref="PlexAccount"/> used to authenticate</param>
        /// <param name="ratingKey">The Plex version of the media id</param>
        /// <returns></returns>
        private async Task<Result<DownloadTask>> CreateDownloadTaskAsync(PlexServer server, int plexAccountId,
            int ratingKey)
        {
            var token = await _plexAuthenticationService.GetPlexServerTokenAsync(plexAccountId, server.Id);

            if (token.IsFailed)
            {
                return token.ToResult<DownloadTask>();
            }

            // TODO make this dynamic
            // Get the download folder
            var downloadFolder = await _mediator.Send(new GetFolderPathByIdQuery(1));
            if (downloadFolder.IsFailed)
            {
                return Result.Fail("the downloadFolder was null");
            }

            // Get the download folder
            var destinationFolder = await _mediator.Send(new GetFolderPathByIdQuery(1));
            if (destinationFolder.IsFailed)
            {
                return Result.Fail("the destinationFolder was null");
            }

            // Retrieve Metadata for this PlexMovie
            var metaData = await _plexApiService.GetMediaMetaDataAsync(token.Value, server.BaseUrl, ratingKey);

            if (metaData != null)
            {
                return Result.Ok(new DownloadTask
                {
                    PlexServerId = server.Id,
                    FolderPathId = downloadFolder.Value.Id,
                    FolderPath = downloadFolder.Value,
                    FileLocationUrl = metaData.ObfuscatedFilePath,
                    Title = metaData.Title,
                    Status = DownloadStatus.Initialized,
                    FileName = metaData.FileName,
                    PlexServerAuthToken = token.Value,
                    DownloadDirectory = _fileSystem.RootDirectory + "/Movies"
                });
            }

            return Result.Fail($"Failed to retrieve metadata for plex media with rating key: {ratingKey}");
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

            return await StartDownloadAsync(downloadTask.Value);
        }

        public async Task<Result<bool>> DownloadTvShowAsync(int plexAccountId, int plexTvShowId, PlexMediaType type)
        {
            var plexTvShow = await _mediator.Send(new GetPlexTvShowByIdWithEpisodesQuery(plexTvShowId));
            if (plexTvShow.IsFailed)
            {
                return plexTvShow.ToResult<bool>();
            }

            var downloadTask = await GetDownloadRequestAsync(plexAccountId, plexTvShow.Value);
            if (downloadTask.IsFailed)
            {
                return downloadTask.ToResult<bool>();
            }

            return await StartDownloadAsync(downloadTask.Value);
        }

        #region CRUD

        public Task<Result<List<DownloadTask>>> GetAllDownloadsAsync()
        {
            return _mediator.Send(new GetAllDownloadTasksQuery());
        }

        public Task<Result<bool>> DeleteDownloadsAsync(int downloadTaskId)
        {
            _downloadManager.CancelDownload(downloadTaskId);
            return _mediator.Send(new DeleteDownloadTaskByIdCommand(downloadTaskId));
        }

        #endregion
    }
}