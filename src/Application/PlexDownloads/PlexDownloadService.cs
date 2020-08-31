#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentResults;
using MediatR;
using PlexRipper.Application.Common;
using PlexRipper.Application.FolderPaths.Queries;
using PlexRipper.Application.PlexDownloads.Commands;
using PlexRipper.Application.PlexDownloads.Queries;
using PlexRipper.Application.PlexMovies;
using PlexRipper.Application.PlexServers.Queries;
using PlexRipper.Application.PlexTvShows.Queries;
using PlexRipper.Domain;
using PlexRipper.Domain.Common;
using PlexRipper.Domain.Entities;
using PlexRipper.Domain.Enums;

#endregion

namespace PlexRipper.Application.PlexDownloads
{
    public class PlexDownloadService : IPlexDownloadService
    {
        #region Fields

        private readonly IDownloadManager _downloadManager;
        private readonly IFileSystem _fileSystem;
        private readonly IFolderPathService _folderPathService;
        private readonly IMediator _mediator;
        private readonly IPlexApiService _plexApiService;
        private readonly IPlexAuthenticationService _plexAuthenticationService;
        private readonly ISignalRService _signalRService;

        #endregion

        #region Constructors

        public PlexDownloadService(IMediator mediator, IDownloadManager downloadManager,
            IPlexAuthenticationService plexAuthenticationService, IFileSystem fileSystem,
            IPlexApiService plexApiService, ISignalRService signalRService, IFolderPathService folderPathService)
        {
            _mediator = mediator;
            _downloadManager = downloadManager;
            _plexAuthenticationService = plexAuthenticationService;
            _fileSystem = fileSystem;
            _plexApiService = plexApiService;
            _signalRService = signalRService;
            _folderPathService = folderPathService;
        }

        #endregion

        #region Methods

        #region Public

        public Task<Result<bool>> DeleteDownloadsAsync(int downloadTaskId)
        {
            _downloadManager.StopDownload(downloadTaskId);
            return _mediator.Send(new DeleteDownloadTaskByIdCommand(downloadTaskId));
        }

        public async Task<Result<bool>> DownloadMovieAsync(int plexAccountId, int plexMovieId)
        {
            Result<PlexMovie> plexMovie = await _mediator.Send(new GetPlexMovieByIdQuery(plexMovieId));
            if (plexMovie.IsFailed) return plexMovie.ToResult<bool>();

            Log.Debug($"Start download setup process for movie: {plexMovie.Value.Title}");
            var downloadTask = await GetDownloadTaskAsync(plexAccountId, plexMovie.Value);
            if (downloadTask.IsFailed) return downloadTask.ToResult<bool>();

            Log.Debug($"Created download task for movie: {plexMovie.Value.Title}");

            return await _downloadManager.AddToDownloadQueueAsync(PrioritizeDownloadTask(downloadTask.Value).Value);
        }

        public async Task<Result<bool>> DownloadTvShowAsync(int plexAccountId, int mediaId, PlexMediaType type)
        {
            List<DownloadTask> downloadTasks = new List<DownloadTask>();
            switch (type)
            {
                case PlexMediaType.TvShow:
                    Result<PlexTvShow> plexTvShow = await _mediator.Send(new GetPlexTvShowByIdWithEpisodesQuery(mediaId));
                    if (plexTvShow.IsFailed) return plexTvShow.ToResult();

                    Result<List<DownloadTask>> y = await GetDownloadTaskAsync(plexAccountId, plexTvShow.Value);
                    if (y.IsFailed) return y.ToResult();

                    downloadTasks = y.Value;
                    break;
                case PlexMediaType.Season:
                    Result<PlexTvShowSeason> plexTvShowSeason = await _mediator.Send(new GetPlexTvShowSeasonByIdWithEpisodesQuery(mediaId));
                    if (plexTvShowSeason.IsFailed) return plexTvShowSeason.ToResult();

                    Result<List<DownloadTask>> x = await GetDownloadTaskAsync(plexAccountId, plexTvShowSeason.Value);
                    if (x.IsFailed) return x.ToResult();

                    downloadTasks = x.Value;
                    break;
                case PlexMediaType.Episode:
                    Result<DownloadTask> downloadTask = await GetDownloadTaskAsync(plexAccountId, mediaId);
                    if (downloadTask.IsFailed) return downloadTask.ToResult();

                    downloadTasks.Add(downloadTask.Value);
                    break;
                default:
                    return Result.Fail("The type has to be either, TvShow, Season or Episode").LogError();
            }
            if (downloadTasks.Count == 0) return Result.Fail("Could not create download tasks").LogError();

            if (downloadTasks.Count == 1) return await _downloadManager.AddToDownloadQueueAsync(downloadTasks[0]);

            return await _downloadManager.AddToDownloadQueueAsync(downloadTasks);
        }


        public Task<Result<List<PlexServer>>> GetAllDownloadsAsync()
        {
            return _mediator.Send(new GetAllDownloadTasksInPlexServersQuery(true, true, true));
        }

        public async Task<Result<DownloadTask>> GetDownloadTaskAsync(int plexAccountId, PlexMovie plexMovie)
        {
            if (plexMovie == null) return ResultExtensions.IsNull(nameof(plexMovie)).LogError();

            if (plexMovie.PlexLibraryId <= 0) return ResultExtensions.IsInvalidId(nameof(plexMovie.PlexLibraryId)).LogWarning();

            Log.Debug($"Creating download request for Movie: {plexMovie.Title}");
            Result<PlexServer> server = await _mediator.Send(new GetPlexServerByPlexLibraryIdQuery(plexMovie.PlexLibraryId));
            if (server.IsFailed) return server.ToResult();

            return await CreateDownloadTaskAsync(server.Value, plexMovie.PlexLibraryId, plexAccountId, plexMovie.RatingKey, PlexMediaType.Movie);
        }

        public Task<string> GetPlexTokenAsync(PlexAccount plexAccount)
        {
            return _plexAuthenticationService.GetPlexTokenAsync(plexAccount);
        }

        public async Task<Result<bool>> RestartDownloadTask(int downloadTaskId)
        {
            if (downloadTaskId <= 0) return ResultExtensions.IsInvalidId(nameof(downloadTaskId), downloadTaskId).LogWarning();

            return await _downloadManager.RestartDownloadAsync(downloadTaskId);
        }

        public Result<bool> StopDownloadTask(int downloadTaskId)
        {
            if (downloadTaskId <= 0) return ResultExtensions.IsInvalidId(nameof(downloadTaskId), downloadTaskId).LogWarning();

            return _downloadManager.StopDownload(downloadTaskId);
        }

        #endregion

        #region Private

        /// <summary>
        /// Creates a <see cref="DownloadTask"/> which can be send to the <see cref="PlexDownloadService"/> to start a download of PlexMedia.
        /// </summary>
        /// <param name="server">The <see cref="PlexServer"/> to download from.</param>
        /// <param name="plexLibraryId">The <see cref="PlexLibrary"/> the media belongs too.</param>
        /// <param name="plexAccountId">The <see cref="PlexAccount"/> used to authenticate.</param>
        /// <param name="ratingKey">The Plex version of the media id.</param>
        /// <param name="mediaType">The media type of the ratingKey.</param>
        /// <returns></returns>
        private async Task<Result<DownloadTask>> CreateDownloadTaskAsync(PlexServer server, int plexLibraryId, int plexAccountId,
            int ratingKey, PlexMediaType mediaType)
        {
            Result<string> token = await _plexAuthenticationService.GetPlexServerTokenAsync(plexAccountId, server.Id);
            if (token.IsFailed) return token.ToResult<DownloadTask>();

            // TODO make this dynamic
            // Get the download folder
            Result<FolderPath> downloadFolder = await _folderPathService.GetDownloadFolderAsync();
            if (downloadFolder.IsFailed) return ResultExtensions.IsNull(nameof(downloadFolder)).LogError();

            // Get the destination folder
            Result<FolderPath> destinationFolder = await _mediator.Send(new GetFolderPathByIdQuery(1));
            if (destinationFolder.IsFailed) return ResultExtensions.IsNull(nameof(destinationFolder)).LogError();

            // Retrieve Metadata for this PlexMovie
            PlexMediaMetaData metaData = await _plexApiService.GetMediaMetaDataAsync(token.Value, server.BaseUrl, ratingKey);
            if (metaData != null)
                return Result.Ok(new DownloadTask
                {
                    PlexServerId = server.Id,
                    DownloadFolderId =  downloadFolder.Value.Id,
                    DownloadFolder = downloadFolder.Value,
                    DestinationFolderId = destinationFolder.Value.Id,
                    DestinationFolder = destinationFolder.Value,
                    PlexAccountId = plexAccountId,
                    FileLocationUrl = metaData.ObfuscatedFilePath,
                    Title = metaData.Title,
                    DownloadStatus = DownloadStatus.Initialized,
                    FileName = metaData.FileName,
                    TitleTvShow = metaData.TitleTvShow,
                    TitleTvShowSeason = metaData.TitleTvShowSeason,
                    RatingKey = metaData.RatingKey,
                    MediaType = mediaType,
                    PlexServer = server,
                    PlexLibraryId = plexLibraryId,
                    Created = DateTime.Now,
                });

            return Result.Fail($"Failed to retrieve metadata for plex media with rating key: {ratingKey}");
        }

        private async Task<Result<List<DownloadTask>>> GetDownloadTaskAsync(int plexAccountId, PlexTvShow plexTvShow)
        {
            if (plexTvShow == null) return ResultExtensions.IsNull(nameof(plexTvShow)).LogError();

            if (plexTvShow.PlexLibraryId <= 0) return ResultExtensions.IsInvalidId(nameof(plexTvShow), plexTvShow.PlexLibraryId).LogWarning();

            if (!plexTvShow.Seasons.Any()) return ResultExtensions.IsEmpty(nameof(plexTvShow.Seasons)).LogWarning();

            Log.Debug($"Creating download tasks for TvShow: {plexTvShow.Title}");
            Result<PlexServer> server = await _mediator.Send(new GetPlexServerByPlexLibraryIdQuery(plexTvShow.PlexLibraryId));
            if (server.IsFailed) return server.ToResult();

            // Parse all contained episodes to DownloadTasks
            List<DownloadTask> downloadTasks = new List<DownloadTask>();
            int i = 0;
            int totalCount = plexTvShow.Seasons.Sum(x => x.Episodes.Count);
            await _signalRService.SendDownloadTaskCreationProgressUpdate(plexTvShow.PlexLibraryId, i, totalCount);
            foreach (PlexTvShowSeason season in plexTvShow.Seasons)
            foreach (PlexTvShowEpisode episode in season.Episodes)
            {
                Result<DownloadTask> downloadTask = await CreateDownloadTaskAsync(server.Value, plexTvShow.PlexLibraryId, plexAccountId, episode.RatingKey,
                    PlexMediaType.Episode);
                if (downloadTask.IsFailed) return downloadTask.ToResult();

                downloadTasks.Add(downloadTask.Value);
                await _signalRService.SendDownloadTaskCreationProgressUpdate(plexTvShow.PlexLibraryId, ++i, totalCount);
            }

            Log.Debug($"Finished creating download tasks for tv show: {plexTvShow.Title}");
            await _signalRService.SendDownloadTaskCreationProgressUpdate(plexTvShow.PlexLibraryId, totalCount, totalCount);
            // Add the priorities to each downloadTask
            return PrioritizeDownloadTasks(downloadTasks);
        }

        private async Task<Result<List<DownloadTask>>> GetDownloadTaskAsync(int plexAccountId, PlexTvShowSeason plexTvShowShowSeason)
        {
            if (plexTvShowShowSeason == null) return ResultExtensions.IsNull(nameof(plexTvShowShowSeason)).LogError();

            Log.Debug($"Creating download request for TvShow season: {plexTvShowShowSeason.Title}");
            Result<PlexServer> server = await _mediator.Send(new GetPlexServerByPlexTvShowSeasonIdQuery(plexTvShowShowSeason.Id));
            if (server.IsFailed) return server.ToResult();

            List<DownloadTask> downloadTasks = new List<DownloadTask>();
            int totalCount = plexTvShowShowSeason.Episodes.Count;
            int index = 0;
            // TODO Wrong Id is passed here, not sure how it will be used
            await _signalRService.SendDownloadTaskCreationProgressUpdate(plexTvShowShowSeason.Id, index, totalCount);
            foreach (PlexTvShowEpisode episode in plexTvShowShowSeason.Episodes)
            {
                // Using GetDownloadRequestAsync(int plexAccountId, int plexTvShowEpisodeId)
                // will result in the same server request being sent multiple times
                Result<DownloadTask> downloadTask = await CreateDownloadTaskAsync(server.Value, plexTvShowShowSeason.PlexLibraryId, plexAccountId, episode.RatingKey,
                    PlexMediaType.Episode);
                if (downloadTask.IsFailed) return downloadTask.ToResult();

                await _signalRService.SendDownloadTaskCreationProgressUpdate(plexTvShowShowSeason.Id, ++index, totalCount);
                downloadTasks.Add(downloadTask.Value);
            }
            await _signalRService.SendDownloadTaskCreationProgressUpdate(plexTvShowShowSeason.Id, totalCount, totalCount);
            // Add the priorities to each downloadTask
            return PrioritizeDownloadTasks(downloadTasks);
        }

        private async Task<Result<DownloadTask>> GetDownloadTaskAsync(int plexAccountId, int plexTvShowEpisodeId)
        {
            Result<PlexTvShowEpisode> plexTvShowEpisode = await _mediator.Send(new GetPlexTvShowEpisodeByIdQuery(plexTvShowEpisodeId));
            if (plexTvShowEpisode.IsFailed) return plexTvShowEpisode.ToResult();

            await _signalRService.SendDownloadTaskCreationProgressUpdate(plexTvShowEpisodeId, 0, 1);
            Result<PlexServer> plexServer = await _mediator.Send(new GetPlexServerByPlexTvShowEpisodeIdQuery(plexTvShowEpisode.Value.Id));
            if (plexServer.IsFailed) return plexServer.ToResult();

            Log.Debug($"Creating download request for TvShow: {plexTvShowEpisode.Value.Title}");

            // TODO Wrong Id is passed here, not sure how it will be used
            await _signalRService.SendDownloadTaskCreationProgressUpdate(plexTvShowEpisodeId, 1, 1);
            var downloadTask = await CreateDownloadTaskAsync(plexServer.Value, plexTvShowEpisode.Value.PlexLibraryId, plexAccountId,
                plexTvShowEpisode.Value.RatingKey, PlexMediaType.Episode);
            // Add the priorities to each downloadTask
            return PrioritizeDownloadTask(downloadTask.Value);
        }

        private Result<DownloadTask> PrioritizeDownloadTask(DownloadTask downloadTask)
        {
            downloadTask.Priority = DataFormat.GetPriority();
            return Result.Ok(downloadTask);
        }

        private Result<List<DownloadTask>> PrioritizeDownloadTasks(List<DownloadTask> downloadTasks)
        {
            var priorities = DataFormat.GetPriority(downloadTasks.Count);
            for (int i = 0; i < downloadTasks.Count; i++)
            {
                downloadTasks[i].Priority = priorities[i];
            }
            return Result.Ok(downloadTasks);
        }

        #endregion

        #endregion
    }
}