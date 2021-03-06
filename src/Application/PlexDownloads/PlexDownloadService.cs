﻿#region

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentResults;
using MediatR;
using PlexRipper.Application.Common;
using PlexRipper.Application.PlexMovies;
using PlexRipper.Application.PlexTvShows;
using PlexRipper.Domain;

#endregion

namespace PlexRipper.Application.PlexDownloads
{
    public class PlexDownloadService : IPlexDownloadService
    {
        #region Fields

        private readonly IDownloadManager _downloadManager;

        private readonly IFileSystem _fileSystem;

        private readonly IFolderPathService _folderPathService;

        private readonly INotificationsService _notificationsService;

        private readonly IMediator _mediator;

        private readonly IPlexApiService _plexApiService;

        private readonly IPlexAuthenticationService _plexAuthenticationService;

        private readonly ISignalRService _signalRService;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PlexDownloadService"/> class.
        /// </summary>
        /// <param name="mediator"></param>
        /// <param name="downloadManager"></param>
        /// <param name="plexAuthenticationService"></param>
        /// <param name="fileSystem"></param>
        /// <param name="plexApiService"></param>
        /// <param name="signalRService"></param>
        /// <param name="folderPathService"></param>
        /// <param name="notificationsService"></param>
        public PlexDownloadService(
            IMediator mediator,
            IDownloadManager downloadManager,
            IPlexAuthenticationService plexAuthenticationService,
            IFileSystem fileSystem,
            IPlexApiService plexApiService,
            ISignalRService signalRService,
            IFolderPathService folderPathService,
            INotificationsService notificationsService)
        {
            _mediator = mediator;
            _downloadManager = downloadManager;
            _plexAuthenticationService = plexAuthenticationService;
            _fileSystem = fileSystem;
            _plexApiService = plexApiService;
            _signalRService = signalRService;
            _folderPathService = folderPathService;
            _notificationsService = notificationsService;
        }

        #endregion

        #region Methods

        #region Private

        /// <summary>
        /// Creates <see cref="DownloadTask"/>s from a <see cref="PlexMovie"/> and send it to the <see cref="IDownloadManager"/>.
        /// </summary>
        /// <param name="plexAccountId">The id of the <see cref="PlexAccount"/> to authenticate with.</param>
        /// <param name="plexMovieId">The id of the <see cref="PlexMovie"/> to create <see cref="DownloadTask"/>s from.</param>
        /// <returns>The created <see cref="DownloadTask"/>.</returns>
        private async Task<Result<bool>> DownloadMovieAsync(int plexMovieId, int plexAccountId = 0)
        {
            Result<PlexMovie> plexMovie = await _mediator.Send(new GetPlexMovieByIdQuery(plexMovieId, true, true));
            if (plexMovie.IsFailed) return plexMovie.ToResult<bool>();

            Log.Debug($"Start download setup process for movie: {plexMovie.Value.Title}");

            var downloadTasks = plexMovie.Value.CreateDownloadTasks();

            Log.Debug($"Created download task(s) for movie: {plexMovie.Value.Title}");

            return await FinalizeDownloadTasks(downloadTasks, plexAccountId);
        }

        private async Task<Result<bool>> DownloadTvShowAsync(int plexTvShowId, int plexAccountId = 0)
        {
            Log.Debug($"Creating download tasks for TvShow with id: {plexTvShowId}");

            var plexTvShow = await _mediator.Send(new GetPlexTvShowByIdWithEpisodesQuery(plexTvShowId, true));
            if (plexTvShow.IsFailed) return plexTvShow.ToResult();

            // Parse all contained episodes to DownloadTasks
            var downloadTasks = plexTvShow.Value.CreateDownloadTasks();

            foreach (var downloadTask in downloadTasks)
            {
                downloadTask.PlexLibrary = plexTvShow.Value.PlexLibrary;
                downloadTask.PlexLibraryId = plexTvShow.Value.PlexLibrary.Id;
                downloadTask.PlexServer = plexTvShow.Value.PlexLibrary.PlexServer;
                downloadTask.PlexServerId = plexTvShow.Value.PlexLibrary.PlexServer.Id;
                downloadTask.TitleTvShowSeason = plexTvShow.Value.Title;
                downloadTask.TitleTvShow = plexTvShow.Value.Title;
            }

            return await FinalizeDownloadTasks(downloadTasks, plexAccountId);
        }

        private async Task<Result<bool>> DownloadTvShowSeasonAsync(int plexTvShowSeasonId, int plexAccountId = 0)
        {
            Log.Debug($"Creating download request for TvShow season with id: {plexTvShowSeasonId}");

            Result<PlexTvShowSeason> plexTvShowSeason = await _mediator.Send(new GetPlexTvShowSeasonByIdWithEpisodesQuery(plexTvShowSeasonId));
            if (plexTvShowSeason.IsFailed)
                return plexTvShowSeason.ToResult();

            var downloadTasks = plexTvShowSeason.Value.CreateDownloadTasks();

            foreach (var downloadTask in downloadTasks)
            {
                downloadTask.PlexLibrary = plexTvShowSeason.Value.PlexLibrary;
                downloadTask.PlexLibraryId = plexTvShowSeason.Value.PlexLibrary.Id;
                downloadTask.PlexServer = plexTvShowSeason.Value.PlexLibrary.PlexServer;
                downloadTask.PlexServerId = plexTvShowSeason.Value.PlexLibrary.PlexServer.Id;
                downloadTask.TitleTvShowSeason = plexTvShowSeason.Value.Title;
                downloadTask.TitleTvShow = plexTvShowSeason.Value.TvShow.Title;
            }

            return await FinalizeDownloadTasks(downloadTasks, plexAccountId);
        }

        private async Task<Result<bool>> DownloadTvShowEpisodeAsync(int plexTvShowEpisodeId, int plexAccountId = 0)
        {
            Log.Debug($"Creating download request for TvShow episode with id: {plexTvShowEpisodeId}");

            var plexTvShowEpisode = await _mediator.Send(new GetPlexTvShowEpisodeByIdQuery(plexTvShowEpisodeId));
            if (plexTvShowEpisode.IsFailed)
                return plexTvShowEpisode.ToResult();

            var downloadTasks = plexTvShowEpisode.Value.CreateDownloadTasks();

            return await FinalizeDownloadTasks(downloadTasks, plexAccountId);
        }

        private async Task<Result<bool>> FinalizeDownloadTasks(List<DownloadTask> downloadTasks, int plexAccountId = 0)
        {
            if (!downloadTasks.Any())
                return ResultExtensions.IsEmpty(nameof(downloadTasks)).LogWarning();

            // Get the download folder
            var downloadFolder = await _folderPathService.GetDownloadFolderAsync();
            if (downloadFolder.IsFailed)
                return downloadFolder.ToResult();

            // Get the destination folder
            var destinationFolder = await _folderPathService.GetDestinationFolderByMediaType(downloadTasks.First().MediaType);
            if (destinationFolder.IsFailed)
                return destinationFolder.ToResult();

            // Get plex server access token
            var serverToken = await _plexAuthenticationService.GetPlexServerTokenAsync(downloadTasks.First().PlexServerId, plexAccountId);
            if (serverToken.IsFailed)
                return serverToken.ToResult();

            foreach (var downloadTask in downloadTasks)
            {
                downloadTask.DownloadFolderId = downloadFolder.Value.Id;
                downloadTask.DownloadFolder = downloadFolder.Value;
                downloadTask.DestinationFolderId = destinationFolder.Value.Id;
                downloadTask.DestinationFolder = destinationFolder.Value;
                downloadTask.ServerToken = serverToken.Value;
            }

            return await _downloadManager.AddToDownloadQueueAsync(downloadTasks);
        }

        private Result<DownloadTask> PrioritizeDownloadTask(DownloadTask downloadTask)
        {
            // TODO This is intended to change the order of downloads, not finished
            downloadTask.Priority = DataFormat.GetPriority();
            return Result.Ok(downloadTask);
        }

        private Result<List<DownloadTask>> PrioritizeDownloadTasks(List<DownloadTask> downloadTasks)
        {
            // TODO This is intended to change the order of downloads, not finished
            var priorities = DataFormat.GetPriority(downloadTasks.Count);
            for (int i = 0; i < downloadTasks.Count; i++)
            {
                downloadTasks[i].Priority = priorities[i];
            }

            return Result.Ok(downloadTasks);
        }

        #endregion

        #region Public

        public Task<Result<List<PlexServer>>> GetDownloadTasksInServerAsync()
        {
            return _mediator.Send(new GetAllDownloadTasksInPlexServersQuery(true));
        }

        public Task<Result<List<DownloadTask>>> GetDownloadTasksAsync()
        {
            return _mediator.Send(new GetAllDownloadTasksQuery());
        }

        public Task<string> GetPlexTokenAsync(PlexAccount plexAccount)
        {
            return _plexAuthenticationService.GetPlexApiTokenAsync(plexAccount);
        }

        #region Commands

        public async Task<Result> DownloadMediaAsync(List<int> mediaIds, PlexMediaType type, int libraryId, int accountId = 0)
        {
            await _signalRService.SendDownloadTaskCreationProgressUpdate(libraryId, 0, mediaIds.Count);
            for (int i = 0; i < mediaIds.Count; i++)
            {
                await DownloadMediaAsync(mediaIds[i], type, accountId);
                await _signalRService.SendDownloadTaskCreationProgressUpdate(libraryId, i, mediaIds.Count);
            }

            await _signalRService.SendDownloadTaskCreationProgressUpdate(libraryId, mediaIds.Count, mediaIds.Count);

            return Result.Ok();
        }

        public async Task<Result<bool>> DownloadMediaAsync(int mediaId, PlexMediaType type, int plexAccountId = 0)
        {
            var result = await _folderPathService.CheckIfFolderPathsAreValid();
            if (result.IsFailed)
            {
                return result;
            }

            switch (type)
            {
                case PlexMediaType.Movie:
                    return await DownloadMovieAsync(mediaId, plexAccountId);
                case PlexMediaType.TvShow:
                    return await DownloadTvShowAsync(mediaId, plexAccountId);
                case PlexMediaType.Season:
                    return await DownloadTvShowSeasonAsync(mediaId, plexAccountId);
                case PlexMediaType.Episode:
                    return await DownloadTvShowEpisodeAsync(mediaId, plexAccountId);
                case PlexMediaType.Music:
                case PlexMediaType.Album:
                    return Result.Fail("PlexMediaType was Music or Album, this is not yet supported").LogWarning();
                case PlexMediaType.None:
                    return Result.Fail("PlexMediaType was none in DownloadMediaAsync").LogWarning();
                case PlexMediaType.Unknown:
                    return Result.Fail("PlexMediaType was Unknown in DownloadMediaAsync").LogWarning();
                default:
                    return Result.Fail($"PlexMediaType defaulted with value {type.ToString()} in DownloadMediaAsync").LogWarning();
            }
        }

        public async Task<Result<bool>> DeleteDownloadTasksAsync(IEnumerable<int> downloadTaskIds)
        {
            return await _downloadManager.DeleteDownloadClients(downloadTaskIds);
        }

        public async Task<Result<bool>> RestartDownloadTask(int downloadTaskId)
        {
            if (downloadTaskId <= 0) return ResultExtensions.IsInvalidId(nameof(downloadTaskId), downloadTaskId).LogWarning();

            return await _downloadManager.RestartDownloadAsync(downloadTaskId);
        }

        public async Task<Result> StopDownloadTask(List<int> downloadTaskIds = null)
        {
            return await _downloadManager.StopDownload(downloadTaskIds);
        }

        public async Task<Result<bool>> StartDownloadTask(int downloadTaskId)
        {
            if (downloadTaskId <= 0) return ResultExtensions.IsInvalidId(nameof(downloadTaskId), downloadTaskId).LogWarning();

            return await _downloadManager.StartDownload(downloadTaskId);
        }

        public async Task<Result> PauseDownloadTask(int downloadTaskId)
        {
            if (downloadTaskId <= 0) return ResultExtensions.IsInvalidId(nameof(downloadTaskId), downloadTaskId).LogWarning();

            return await _downloadManager.PauseDownload(downloadTaskId);
        }

        public Task<Result<bool>> ClearCompleted(List<int> downloadTaskIds)
        {
            return _downloadManager.ClearCompletedAsync(downloadTaskIds);
        }

        #endregion

        #endregion

        #endregion
    }
}