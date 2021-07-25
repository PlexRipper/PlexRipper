using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentResults;
using MediatR;
using PlexRipper.Application.Common;
using PlexRipper.Application.PlexMedia;
using PlexRipper.Application.PlexMovies;
using PlexRipper.Application.PlexTvShows;
using PlexRipper.Domain;

namespace PlexRipper.Application
{
    public class PlexDownloadTaskFactory : IPlexDownloadTaskFactory
    {
        private readonly IMediator _mediator;

        private readonly IPlexAuthenticationService _plexAuthenticationService;

        private readonly INotificationsService _notificationsService;

        private readonly IFolderPathService _folderPathService;

        private readonly IUserSettings _userSettings;

        public PlexDownloadTaskFactory(
            IMediator mediator,
            IPlexAuthenticationService plexAuthenticationService,
            INotificationsService notificationsService,
            IFolderPathService folderPathService,
            IUserSettings userSettings)
        {
            _mediator = mediator;
            _plexAuthenticationService = plexAuthenticationService;
            _notificationsService = notificationsService;

            _folderPathService = folderPathService;
            _userSettings = userSettings;
        }

        public async Task<Result<List<DownloadTask>>> GenerateAsync(List<int> mediaIds, PlexMediaType type)
        {
            var result = await _folderPathService.CheckIfFolderPathsAreValid();
            if (result.IsFailed)
            {
                return result.LogError();
            }

            switch (type)
            {
                case PlexMediaType.Movie:
                    return await GenerateMovieDownloadTasksAsync(mediaIds);
                case PlexMediaType.TvShow:
                    return await GenerateDownloadTvShowTasksAsync(mediaIds);
                case PlexMediaType.Season:
                    return await GenerateDownloadTvShowSeasonTasksAsync(mediaIds);
                case PlexMediaType.Episode:
                    return await GenerateDownloadTvShowEpisodeTasksAsync(mediaIds);
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

        #region Private Methods

        /// <summary>
        /// Creates <see cref="DownloadTask"/>s from a <see cref="PlexMovie"/> and send it to the <see cref="IDownloadManager"/>.
        /// </summary>
        /// <param name="plexMovieIds">The ids of the <see cref="PlexMovie"/> to create <see cref="DownloadTask"/>s from.</param>
        /// <returns>The created <see cref="DownloadTask"/>.</returns>
        public async Task<Result<List<DownloadTask>>> GenerateMovieDownloadTasksAsync(List<int> plexMovieIds)
        {
            Log.Debug($"Creating {plexMovieIds.Count} movie download tasks.");
            var plexMoviesResult = await _mediator.Send(new GetMultiplePlexMoviesByIdsQuery(plexMovieIds, true, true));

            if (plexMoviesResult.IsFailed)
                return plexMoviesResult.ToResult();

            var downloadTasks = new List<DownloadTask>();
            foreach (var plexMovie in plexMoviesResult.Value)
            {
                downloadTasks.AddRange(plexMovie.CreateDownloadTasks());

                Log.Debug($"Created download task(s) for movie: {plexMovie.Title}");
            }

            var finalizeDownloadTasksResult = await FinalizeDownloadTasks(downloadTasks);
            if (finalizeDownloadTasksResult.IsFailed)
            {
                return finalizeDownloadTasksResult.ToResult().LogError();
            }

            return Result.Ok(finalizeDownloadTasksResult.Value);
        }

        public async Task<Result<List<DownloadTask>>> GenerateDownloadTvShowTasksAsync(List<int> plexTvShowIds)
        {
            Log.Debug($"Creating download tasks for TvShow with id: {plexTvShowIds}");

            var plexTvShows = await _mediator.Send(new GetMultiplePlexTvShowsByIdsWithEpisodesQuery(plexTvShowIds, true, true, true));
            if (plexTvShows.IsFailed) return plexTvShows.ToResult();

            // Parse all contained episodes to DownloadTasks
            var downloadTasks = new List<DownloadTask>();
            foreach (var plexTvShow in plexTvShows.Value)
            {
                var tvShowDownloadTasks = plexTvShow.CreateDownloadTasks();
                foreach (DownloadTask downloadTask in tvShowDownloadTasks)
                {
                    downloadTask.PlexLibrary = plexTvShow.PlexLibrary;
                    downloadTask.PlexServer = plexTvShow.PlexServer;
                }

                downloadTasks.AddRange(tvShowDownloadTasks);
                Log.Debug($"Created download task(s) for tvShow: {plexTvShow.Title}");
            }

            var finalizeDownloadTasksResult = await FinalizeDownloadTasks(downloadTasks);
            if (finalizeDownloadTasksResult.IsFailed)
            {
                return finalizeDownloadTasksResult.ToResult().LogError();
            }

            return Result.Ok(finalizeDownloadTasksResult.Value);
        }

        public async Task<Result<List<DownloadTask>>> GenerateDownloadTvShowSeasonTasksAsync(List<int> plexTvShowSeasonIds)
        {
            Log.Debug($"Creating download request for TvShow season with id: {plexTvShowSeasonIds}");

            var plexTvShowSeasonResult =
                await _mediator.Send(new GetMultiplePlexTvShowSeasonsByIdsWithEpisodesQuery(plexTvShowSeasonIds, true, true, true));
            if (plexTvShowSeasonResult.IsFailed)
                return plexTvShowSeasonResult.ToResult();

            var downloadTasks = new List<DownloadTask>();
            foreach (var plexTvShowSeason in plexTvShowSeasonResult.Value)
            {
                var seasonDownloadTasks = plexTvShowSeason.CreateDownloadTasks();
                foreach (DownloadTask downloadTask in seasonDownloadTasks)
                {
                    downloadTask.PlexLibrary = plexTvShowSeason.PlexLibrary;
                    downloadTask.PlexServer = plexTvShowSeason.PlexServer;
                }

                downloadTasks.AddRange(seasonDownloadTasks);
                Log.Debug($"Created download task(s) for tvShowSeasons: {plexTvShowSeason.Title}");
            }

            var finalizeDownloadTasksResult = await FinalizeDownloadTasks(downloadTasks);
            if (finalizeDownloadTasksResult.IsFailed)
            {
                return finalizeDownloadTasksResult.ToResult().LogError();
            }

            return Result.Ok(finalizeDownloadTasksResult.Value);
        }

        public async Task<Result<List<DownloadTask>>> GenerateDownloadTvShowEpisodeTasksAsync(List<int> plexTvShowEpisodeId)
        {
            Log.Debug($"Creating download request for TvShow episode with id: {plexTvShowEpisodeId}");

            var plexTvShowEpisodeResult = await _mediator.Send(new GetMultiplePlexTvShowEpisodesByIdQuery(plexTvShowEpisodeId, true, true, true));
            if (plexTvShowEpisodeResult.IsFailed)
                return plexTvShowEpisodeResult.ToResult();

            var downloadTasks = new List<DownloadTask>();
            foreach (var plexTvShowSeason in plexTvShowEpisodeResult.Value)
            {
                downloadTasks.AddRange(plexTvShowSeason.CreateDownloadTasks());

                Log.Debug($"Created download task(s) for tvShowEpisodes: {plexTvShowSeason.Title}");
            }

            var finalizeDownloadTasksResult = await FinalizeDownloadTasks(downloadTasks);
            if (finalizeDownloadTasksResult.IsFailed)
            {
                return finalizeDownloadTasksResult.ToResult().LogError();
            }

            return Result.Ok(finalizeDownloadTasksResult.Value);
        }

        public async Task<Result<List<DownloadTask>>> FinalizeDownloadTasks(List<DownloadTask> downloadTasks, int plexAccountId = 0)
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

            var parts = _userSettings.AdvancedSettings.DownloadManager.DownloadSegments;

            foreach (var downloadTask in downloadTasks)
            {
                downloadTask.DownloadFolderId = downloadFolder.Value.Id;
                downloadTask.DownloadFolder = downloadFolder.Value;
                downloadTask.DestinationFolderId = destinationFolder.Value.Id;
                downloadTask.DestinationFolder = destinationFolder.Value;
                downloadTask.ServerToken = serverToken.Value;

                downloadTask.DownloadWorkerTasks = GenerateDownloadWorkerTasks(downloadTask, parts);
            }

            return Result.Ok(downloadTasks);
        }

        public List<DownloadWorkerTask> GenerateDownloadWorkerTasks(DownloadTask downloadTask, int parts)
        {
            // Create download worker tasks/segments/ranges
            var totalBytesToReceive = downloadTask.DataTotal;
            var partSize = totalBytesToReceive / parts;
            var remainder = totalBytesToReceive - partSize * parts;

            var downloadWorkerTasks = new List<DownloadWorkerTask>();

            for (int i = 0; i < parts; i++)
            {
                long start = partSize * i;
                long end = start + partSize;
                if (i == parts - 1 && remainder > 0)
                {
                    // Add the remainder to the last download range
                    end += remainder;
                }

                downloadWorkerTasks.Add(new DownloadWorkerTask(downloadTask, i + 1, start, end));
            }

            return downloadWorkerTasks;
        }

        /// <inheritdoc/>
        public async Task<Result<List<DownloadTask>>> RegenerateDownloadTask(List<DownloadTask> downloadTasks)
        {
            if (downloadTasks is null || !downloadTasks.Any())
            {
                return Result.Fail("Parameter downloadTasks was empty or null").LogError();
            }

            Log.Debug($"Regenerating {downloadTasks.Count} download tasks.");

            var freshDownloadTasks = new List<DownloadTask>();

            foreach (var downloadTask in downloadTasks)
            {
                var mediaIdResult =
                    await _mediator.Send(new GetPlexMediaIdByKeyQuery(downloadTask.Key, downloadTask.MediaType, downloadTask.PlexServerId));
                if (mediaIdResult.IsFailed)
                {
                    var result = Result.Fail($"Could not recreate the download task for {downloadTask.TitlePath}");
                    result.WithReasons(mediaIdResult.Reasons);
                    await _notificationsService.SendResult(result);
                    continue;
                }

                var downloadTasksResult = await GenerateAsync(new List<int> { mediaIdResult.Value }, downloadTask.MediaType);
                if (downloadTasksResult.IsFailed)
                {
                    var result = Result.Fail($"Could not recreate the download task for {downloadTask.TitlePath}");
                    result.WithReasons(mediaIdResult.Reasons);
                    await _notificationsService.SendResult(result);
                    continue;
                }

                await _mediator.Send(new DeleteDownloadWorkerTasksByDownloadTaskIdCommand(downloadTask.Id));

                //TODO Certain properties should be copied over such as priority to maintain the same order in the front-end
                downloadTasksResult.Value[0].Id = downloadTask.Id;
                downloadTasksResult.Value[0].Priority = downloadTask.Priority;
                downloadTasksResult.Value[0].DownloadWorkerTasks.ForEach(x => x.DownloadTaskId = downloadTask.Id);


                freshDownloadTasks.AddRange(downloadTasksResult.Value);
            }

            Log.Debug($"Successfully regenerated {freshDownloadTasks.Count} out of {downloadTasks.Count} download tasks.");
            if (downloadTasks.Count - freshDownloadTasks.Count > 0)
            {
                Log.Error("Failed to generate");
            }
            return Result.Ok(freshDownloadTasks);
        }

        #endregion
    }
}