using System;
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
using System.Linq;
using System.Threading.Tasks;
using PlexRipper.Application.PlexServers.Queries;
using PlexRipper.Application.PlexTvShows.Queries;
using PlexRipper.Domain.FluentResultExtensions;

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

        public async Task<Result<DownloadTask>> GetDownloadTaskAsync(int plexAccountId, PlexMovie plexMovie)
        {
            if (plexMovie == null)
            {
                return ResultExtensions.IsNull(nameof(plexMovie)).LogError();
            }

            if (plexMovie.PlexLibraryId <= 0)
            {
                return ResultExtensions.IsInvalidId(nameof(plexMovie.PlexLibraryId)).LogWarning();
            }

            Log.Debug($"Creating download request for Movie: {plexMovie.Title}");

            var server = await _mediator.Send(new GetPlexServerByPlexLibraryIdQuery(plexMovie.PlexLibraryId));

            if (server.IsFailed)
            {
                return server.ToResult();
            }
            return await CreateDownloadTaskAsync(server.Value, plexAccountId, plexMovie.RatingKey);
        }

        private async Task<Result<List<DownloadTask>>> GetDownloadTaskAsync(int plexAccountId, PlexTvShow plexTvShow)
        {
            if (plexTvShow == null)
            {
                return ResultExtensions.IsNull(nameof(plexTvShow)).LogError();
            }

            if (plexTvShow.PlexLibraryId <= 0)
            {
                return ResultExtensions.IsInvalidId(nameof(plexTvShow), plexTvShow.PlexLibraryId).LogWarning();
            }

            if (!plexTvShow.Seasons.Any())
            {
                return ResultExtensions.IsEmpty(nameof(plexTvShow.Seasons)).LogWarning();
            }

            Log.Debug($"Creating download request for TvShow: {plexTvShow.Title}");

            var server = await _mediator.Send(new GetPlexServerByPlexLibraryIdQuery(plexTvShow.PlexLibraryId));
            if (server.IsFailed)
            {
                return server.ToResult();
            }

            // Parse all contained episodes to DownloadTasks
            var downloadTasks = new List<DownloadTask>();
            foreach (var season in plexTvShow.Seasons)
            {
                foreach (var episode in season.Episodes)
                {
                    var downloadTask = await CreateDownloadTaskAsync(server.Value, plexAccountId, episode.RatingKey);
                    if (downloadTask.IsFailed)
                    {
                        return downloadTask.ToResult();
                    }
                    downloadTasks.Add(downloadTask.Value);
                }
            }
            return Result.Ok(downloadTasks);
        }

        private async Task<Result<List<DownloadTask>>> GetDownloadTaskAsync(int plexAccountId, PlexTvShowSeason plexTvShowShowSeason)
        {
            if (plexTvShowShowSeason == null)
            {
                return ResultExtensions.IsNull(nameof(plexTvShowShowSeason)).LogError();
            }

            Log.Debug($"Creating download request for TvShow season: {plexTvShowShowSeason.Title}");

            var server = await _mediator.Send(new GetPlexServerByPlexTvShowSeasonIdQuery(plexTvShowShowSeason.Id));
            if (server.IsFailed)
            {
                return server.ToResult();
            }

            var downloadTasks = new List<DownloadTask>();
            foreach (var episode in plexTvShowShowSeason.Episodes)
            {
                // Using GetDownloadRequestAsync(int plexAccountId, int plexTvShowEpisodeId)
                // will result in the same server request being sent multiple times
                var downloadTask = await CreateDownloadTaskAsync(server.Value, plexAccountId, episode.RatingKey);
                if (downloadTask.IsFailed)
                {
                    return downloadTask.ToResult();
                }
                downloadTasks.Add(downloadTask.Value);
            }

            return Result.Ok(downloadTasks);
        }

        private async Task<Result<DownloadTask>> GetDownloadTaskAsync(int plexAccountId, int plexTvShowEpisodeId)
        {
            var plexTvShowEpisode = await _mediator.Send(new GetPlexTvShowEpisodeByIdQuery(plexTvShowEpisodeId));
            if (plexTvShowEpisode.IsFailed)
            {
                return plexTvShowEpisode.ToResult();
            }

            var plexServer = await _mediator.Send(new GetPlexServerByPlexTvShowEpisodeIdQuery(plexTvShowEpisode.Value.Id));
            if (plexServer.IsFailed)
            {
                return plexServer.ToResult();
            }

            Log.Debug($"Creating download request for TvShow: {plexTvShowEpisode.Value.Title}");

            return await CreateDownloadTaskAsync(plexServer.Value, plexAccountId, plexTvShowEpisode.Value.RatingKey);
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
                return ResultExtensions.IsNull(nameof(downloadFolder)).LogError();
            }

            // Get the download folder
            var destinationFolder = await _mediator.Send(new GetFolderPathByIdQuery(1));
            if (destinationFolder.IsFailed)
            {
                return ResultExtensions.IsNull(nameof(destinationFolder)).LogError();
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
                    DownloadDirectory = downloadFolder.Value.Directory
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

            var downloadTask = await GetDownloadTaskAsync(plexAccountId, plexMovie.Value);
            if (downloadTask.IsFailed)
            {
                return downloadTask.ToResult<bool>();
            }

            return await StartDownloadAsync(downloadTask.Value);
        }

        public async Task<Result<bool>> DownloadTvShowAsync(int plexAccountId, int mediaId, PlexMediaType type)
        {
            var downloadTasks = new List<DownloadTask>();

            switch (type)
            {
                case PlexMediaType.TvShow:
                    var plexTvShow = await _mediator.Send(new GetPlexTvShowByIdWithEpisodesQuery(mediaId));
                    break;

                case PlexMediaType.Season:
                    var plexTvShowSeason = await _mediator.Send(new GetPlexTvShowSeasonByIdWithEpisodesQuery(mediaId));
                    if (plexTvShowSeason.IsFailed)
                    {
                        return plexTvShowSeason.ToResult();
                    }
                    var x = await GetDownloadTaskAsync(plexAccountId, plexTvShowSeason.Value);
                    if (x.IsFailed)
                    {
                        return x.ToResult();
                    }
                    break;

                case PlexMediaType.Episode:

                    var downloadTask = await GetDownloadTaskAsync(plexAccountId, mediaId);
                    if (downloadTask.IsFailed)
                    {
                        return downloadTask.ToResult();
                    }
                    downloadTasks.Add(downloadTask.Value);
                    break;

                default:
                    return Result.Fail("The type has to be either, TvShow, Season or Episode").LogError();
            }

            if (downloadTasks.Count == 0)
            {
                return Result.Fail("Could not create download tasks").LogError();
            }

            if (downloadTasks.Count == 1)
            {
                return await _downloadManager.StartDownloadAsync(downloadTasks[0]);
            }

            return await _downloadManager.StartDownloadAsync(downloadTasks);
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