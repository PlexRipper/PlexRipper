#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentResults;
using MediatR;
using PlexRipper.Application.Common;
using PlexRipper.Application.FolderPaths.Queries;
using PlexRipper.Application.PlexMovies;
using PlexRipper.Application.PlexServers;
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

        private readonly IMediator _mediator;

        private readonly IPlexApiService _plexApiService;

        private readonly IPlexAuthenticationService _plexAuthenticationService;

        private readonly ISignalRService _signalRService;

        #endregion

        #region Constructors

        public PlexDownloadService(
            IMediator mediator,
            IDownloadManager downloadManager,
            IPlexAuthenticationService plexAuthenticationService,
            IFileSystem fileSystem,
            IPlexApiService plexApiService,
            ISignalRService signalRService,
            IFolderPathService folderPathService)
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

        #region Private

        /// <summary>
        /// Creates <see cref="DownloadTask"/>s from a <see cref="PlexMovie"/> and send it to the <see cref="IDownloadManager"/>.
        /// </summary>
        /// <param name="plexAccountId">The id of the <see cref="PlexAccount"/> to authenticate with.</param>
        /// <param name="plexMovieId">The id of the <see cref="PlexMovie"/> to create <see cref="DownloadTask"/>s from.</param>
        /// <returns>The created <see cref="DownloadTask"/>.</returns>
        private async Task<Result<bool>> DownloadMovieAsync(int plexAccountId, int plexMovieId)
        {
            Result<PlexMovie> plexMovie = await _mediator.Send(new GetPlexMovieByIdQuery(plexMovieId, true, true));
            if (plexMovie.IsFailed) return plexMovie.ToResult<bool>();

            Log.Debug($"Start download setup process for movie: {plexMovie.Value.Title}");

            var downloadTasks = plexMovie.Value.CreateDownloadTasks();

            // Get the download folder
            Result<FolderPath> downloadFolder = await _folderPathService.GetDownloadFolderAsync();
            if (downloadFolder.IsFailed)
                return downloadFolder.ToResult();

            // Get the destination folder
            Result<FolderPath> destinationFolder = await _folderPathService.GetMovieDestinationFolderAsync();
            if (destinationFolder.IsFailed)
                return destinationFolder.ToResult();

            foreach (var downloadTask in downloadTasks)
            {
                downloadTask.PlexAccountId = plexAccountId;
                downloadTask.DownloadFolderId = downloadFolder.Value.Id;
                downloadTask.DownloadFolder = downloadFolder.Value;
                downloadTask.DestinationFolderId = destinationFolder.Value.Id;
                downloadTask.DestinationFolder = destinationFolder.Value;
            }

            Log.Debug($"Created download task(s) for movie: {plexMovie.Value.Title}");

            return await FinalizeDownloadTasks(downloadTasks);
        }

        private async Task<Result<bool>> FinalizeDownloadTasks(List<DownloadTask> downloadTasks)
        {
            // Add to Database
            var createResult = await _mediator.Send(new CreateDownloadTasksCommand(downloadTasks));
            if (createResult.IsFailed)
            {
                return createResult.ToResult().LogError();
            }

            // Set the Id's of the just added downloadTasks
            for (int i = 0; i < downloadTasks.Count; i++)
            {
                downloadTasks[i].Id = createResult.Value[i];
            }

            return await _downloadManager.AddToDownloadQueueAsync(downloadTasks);
        }

        private async Task<Result<bool>> DownloadTvShowAsync(int plexAccountId, int mediaId, PlexMediaType type)
        {
            List<DownloadTask> downloadTasks = new List<DownloadTask>();
            switch (type)
            {
                case PlexMediaType.TvShow:
                    Result<PlexTvShow> plexTvShow = await _mediator.Send(new GetPlexTvShowByIdWithEpisodesQuery(mediaId));
                    if (plexTvShow.IsFailed) return plexTvShow.ToResult();

                    Result<List<DownloadTask>> y = await CreateDownloadTaskAsync(plexAccountId, plexTvShow.Value);
                    if (y.IsFailed) return y.ToResult();

                    downloadTasks = y.Value;
                    break;
                case PlexMediaType.Season:
                    Result<PlexTvShowSeason> plexTvShowSeason = await _mediator.Send(new GetPlexTvShowSeasonByIdWithEpisodesQuery(mediaId));
                    if (plexTvShowSeason.IsFailed) return plexTvShowSeason.ToResult();

                    Result<List<DownloadTask>> x = await CreateDownloadTaskAsync(plexAccountId, plexTvShowSeason.Value);
                    if (x.IsFailed) return x.ToResult();

                    downloadTasks = x.Value;
                    break;
                case PlexMediaType.Episode:
                    Result<DownloadTask> downloadTask = await CreateDownloadTaskAsync(plexAccountId, mediaId);
                    if (downloadTask.IsFailed) return downloadTask.ToResult();

                    downloadTasks.Add(downloadTask.Value);
                    break;
                default:
                    return Result.Fail("The type has to be either, TvShow, Season or Episode").LogError();
            }

            if (downloadTasks.Count == 0) return Result.Fail("Could not create download tasks").LogError();

            return await _downloadManager.AddToDownloadQueueAsync(downloadTasks);
        }

        /// <summary>
        /// Creates a <see cref="DownloadTask"/> which can be send to the <see cref="PlexDownloadService"/> to start a download of PlexMedia.
        /// </summary>
        /// <param name="server">The <see cref="PlexServer"/> to download from.</param>
        /// <param name="plexLibraryId">The <see cref="PlexLibrary"/> the media belongs too.</param>
        /// <param name="plexAccountId">The <see cref="PlexAccount"/> used to authenticate.</param>
        /// <param name="ratingKey">The Plex version of the media id.</param>
        /// <param name="mediaType">The media type of the ratingKey.</param>
        /// <returns></returns>
        private async Task<Result<DownloadTask>> CreateDownloadTaskAsync(PlexServer server, int plexLibraryId, int plexAccountId, int ratingKey,
            PlexMediaType mediaType)
        {
            Result<string> token = await _plexAuthenticationService.GetPlexServerTokenAsync(plexAccountId, server.Id);
            if (token.IsFailed) return token.ToResult<DownloadTask>();

            // TODO make this dynamic
            // Get the download folder
            Result<FolderPath> downloadFolder = await _folderPathService.GetDownloadFolderAsync();
            if (downloadFolder.IsFailed) return downloadFolder.ToResult();

            // Get the destination folder
            Result<FolderPath> destinationFolder = await _mediator.Send(new GetFolderPathByIdQuery(1));
            if (destinationFolder.IsFailed) return destinationFolder.ToResult();

            // Retrieve Metadata for this PlexMovie
            PlexMediaMetaData metaData = await _plexApiService.GetMediaMetaDataAsync(token.Value, server.BaseUrl, ratingKey);
            if (metaData != null)
            {
                return Result.Ok(new DownloadTask
                {
                    PlexServerId = server.Id,
                    DownloadFolderId = downloadFolder.Value.Id,
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
            }

            return Result.Fail($"Failed to retrieve metadata for plex media with rating key: {ratingKey}");
        }

        private async Task<Result<List<DownloadTask>>> CreateDownloadTaskAsync(int plexAccountId, PlexTvShow plexTvShow)
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
            {
                foreach (PlexTvShowEpisode episode in season.Episodes)
                {
                    Result<DownloadTask> downloadTask = await CreateDownloadTaskAsync(server.Value, plexTvShow.PlexLibraryId, plexAccountId,
                        episode.RatingKey,
                        PlexMediaType.Episode);
                    if (downloadTask.IsFailed) return downloadTask.ToResult();

                    downloadTasks.Add(downloadTask.Value);
                    await _signalRService.SendDownloadTaskCreationProgressUpdate(plexTvShow.PlexLibraryId, ++i, totalCount);
                }
            }

            Log.Debug($"Finished creating download tasks for tv show: {plexTvShow.Title}");
            await _signalRService.SendDownloadTaskCreationProgressUpdate(plexTvShow.PlexLibraryId, totalCount, totalCount);

            // Add the priorities to each downloadTask
            return PrioritizeDownloadTasks(downloadTasks);
        }

        private async Task<Result<List<DownloadTask>>> CreateDownloadTaskAsync(int plexAccountId, PlexTvShowSeason plexTvShowShowSeason)
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
                Result<DownloadTask> downloadTask = await CreateDownloadTaskAsync(server.Value, plexTvShowShowSeason.PlexLibraryId, plexAccountId,
                    episode.RatingKey,
                    PlexMediaType.Episode);
                if (downloadTask.IsFailed) return downloadTask.ToResult();

                await _signalRService.SendDownloadTaskCreationProgressUpdate(plexTvShowShowSeason.Id, ++index, totalCount);
                downloadTasks.Add(downloadTask.Value);
            }

            await _signalRService.SendDownloadTaskCreationProgressUpdate(plexTvShowShowSeason.Id, totalCount, totalCount);

            // Add the priorities to each downloadTask
            return PrioritizeDownloadTasks(downloadTasks);
        }

        private async Task<Result<DownloadTask>> CreateDownloadTaskAsync(int plexAccountId, int plexTvShowEpisodeId)
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

        public Task<Result<List<PlexServer>>> GetAllDownloadsAsync()
        {
            return _mediator.Send(new GetAllDownloadTasksInPlexServersQuery(true, true));
        }

        public Task<string> GetPlexTokenAsync(PlexAccount plexAccount)
        {
            return _plexAuthenticationService.GetPlexTokenAsync(plexAccount);
        }

        #region Commands

        public Task<Result<bool>> ClearCompleted()
        {
            return _downloadManager.ClearCompletedAsync();
        }

        public async Task<Result<bool>> DownloadMediaAsync(int plexAccountId, int mediaId, PlexMediaType type)
        {
            var result = await _folderPathService.CheckIfFolderPathsAreValid();
            if (result.IsFailed)
            {
                return result;
            }

            switch (type)
            {
                case PlexMediaType.None:
                    return Result.Fail("PlexMediaType was none in DownloadMediaAsync").LogWarning();
                case PlexMediaType.Movie:
                    return await DownloadMovieAsync(plexAccountId, mediaId);
                case PlexMediaType.TvShow:
                case PlexMediaType.Season:
                case PlexMediaType.Episode:
                    return await DownloadTvShowAsync(plexAccountId, mediaId, type);
                case PlexMediaType.Music:
                case PlexMediaType.Album:
                    return Result.Fail("PlexMediaType was Music or Album, this is not yet supported").LogWarning();
                case PlexMediaType.Unknown:
                    return Result.Fail("PlexMediaType was Unknown in DownloadMediaAsync").LogWarning();
                default:
                    return Result.Fail($"PlexMediaType defaulted with value {type.ToString()} in DownloadMediaAsync").LogWarning();
            }
        }

        public async Task<Result<bool>> DeleteDownloadTaskAsync(int downloadTaskId)
        {
            if (downloadTaskId <= 0) return ResultExtensions.IsInvalidId(nameof(downloadTaskId), downloadTaskId).LogWarning();

            return await _downloadManager.DeleteDownloadClient(downloadTaskId);
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

        public async Task<Result<bool>> StopDownloadTask(int downloadTaskId)
        {
            if (downloadTaskId <= 0) return ResultExtensions.IsInvalidId(nameof(downloadTaskId), downloadTaskId).LogWarning();

            return await _downloadManager.StopDownloadAsync(downloadTaskId);
        }

        public async Task<Result<bool>> StartDownloadTask(int downloadTaskId)
        {
            if (downloadTaskId <= 0) return ResultExtensions.IsInvalidId(nameof(downloadTaskId), downloadTaskId).LogWarning();

            return await _downloadManager.StartDownload(downloadTaskId);
        }

        public Result<bool> PauseDownloadTask(int downloadTaskId)
        {
            if (downloadTaskId <= 0) return ResultExtensions.IsInvalidId(nameof(downloadTaskId), downloadTaskId).LogWarning();

            return _downloadManager.PauseDownload(downloadTaskId);
        }

        #endregion

        #endregion

        #endregion
    }
}