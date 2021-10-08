using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FluentResultExtensions.lib;
using FluentResults;
using Logging;
using MediatR;
using PlexRipper.Application.Common;
using PlexRipper.Application.Common.WebApi;
using PlexRipper.Application.PlexMedia;
using PlexRipper.Application.PlexMovies;
using PlexRipper.Application.PlexTvShows;
using PlexRipper.Domain;

namespace PlexRipper.Application
{
    public class PlexDownloadTaskFactory : IPlexDownloadTaskFactory
    {
        private readonly IMediator _mediator;

        private readonly IMapper _mapper;

        private readonly IPlexAuthenticationService _plexAuthenticationService;

        private readonly INotificationsService _notificationsService;

        private readonly IFolderPathService _folderPathService;

        private readonly IUserSettings _userSettings;

        public PlexDownloadTaskFactory(
            IMediator mediator,
            IMapper mapper,
            IPlexAuthenticationService plexAuthenticationService,
            INotificationsService notificationsService,
            IFolderPathService folderPathService,
            IUserSettings userSettings)
        {
            _mediator = mediator;
            _mapper = mapper;
            _plexAuthenticationService = plexAuthenticationService;
            _notificationsService = notificationsService;

            _folderPathService = folderPathService;
            _userSettings = userSettings;
        }

        public async Task<Result<List<DownloadTask>>> GenerateAsync(List<DownloadMediaDTO> downloadMedias)
        {
            var resultFolderPath = await _folderPathService.CheckIfFolderPathsAreValid();
            if (resultFolderPath.IsFailed)
            {
                return resultFolderPath.LogError();
            }

            var downloadTasks = new List<DownloadTask>();

            var plexTvShowIds = downloadMedias.FindAll(x => x.Type == PlexMediaType.TvShow).SelectMany(x => x.MediaIds).ToList();
            var plexTvShowSeasonIds = downloadMedias.FindAll(x => x.Type == PlexMediaType.Season).SelectMany(x => x.MediaIds).ToList();
            var plexTvShowEpisodeIds = downloadMedias.FindAll(x => x.Type == PlexMediaType.Episode).SelectMany(x => x.MediaIds).ToList();
            var plexMovieIds = downloadMedias.FindAll(x => x.Type == PlexMediaType.Movie).SelectMany(x => x.MediaIds).ToList();

            if (plexTvShowIds.Any() || plexTvShowSeasonIds.Any() || plexTvShowEpisodeIds.Any())
            {
                var result = await GenerateDownloadTvShowTasksAsync(plexTvShowIds, plexTvShowSeasonIds, plexTvShowEpisodeIds);
                if (result.IsSuccess)
                {
                    downloadTasks.AddRange(result.Value);
                }
                else
                {
                    result.LogError();
                }
            }

            if (plexMovieIds.Any())
            {
                var result = await GenerateMovieDownloadTasksAsync(plexMovieIds);
                if (result.IsSuccess)
                {
                    downloadTasks.AddRange(result.Value);
                }
                else
                {
                    result.LogError();
                }
            }

            return Result.Ok(downloadTasks);
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

            var downloadTasks = CreateDownloadTasks(plexMoviesResult.Value);

            var finalizeDownloadTasksResult = await FinalizeDownloadTasks(downloadTasks.Value);
            if (finalizeDownloadTasksResult.IsFailed)
            {
                return finalizeDownloadTasksResult.ToResult().LogError();
            }

            return Result.Ok(finalizeDownloadTasksResult.Value);
        }

        #region Create DownloadTasks

        public Result<List<DownloadTask>> CreateDownloadTasks(List<PlexMovie> movies)
        {
            if (!movies.Any())
            {
                return ResultExtensions.IsEmpty(nameof(movies));
            }

            static DownloadTask CreateBaseTask(PlexMovie plexMovie)
            {
                return new()
                {
                    MediaType = PlexMediaType.Movie,
                    DownloadTaskType = DownloadTaskType.Movie,
                    DownloadStatus = DownloadStatus.Initialized,
                    Created = DateTime.UtcNow,
                    Title = plexMovie.Title,
                    Year = plexMovie.Year,
                    PlexLibrary = plexMovie.PlexLibrary,
                    PlexLibraryId = plexMovie.PlexLibraryId,
                    PlexServer = plexMovie.PlexServer,
                    PlexServerId = plexMovie.PlexServerId,
                    Key = plexMovie.Key,
                    Children = new List<DownloadTask>(),
                    MediaId = plexMovie.Id,
                    DataTotal = 0,
                };
            }

            var downloadTasks = new List<DownloadTask>();
            foreach (var plexMovie in movies)
            {
                var movieDownloadTask = _mapper.Map<DownloadTask>(plexMovie);

                // TODO Add quality chooser, will download everything now
                foreach (var movieData in plexMovie.MovieData)
                {
                    var movieDataDownloadTask = CreateBaseTask(plexMovie);
                    movieDataDownloadTask.DownloadTaskType = DownloadTaskType.MovieData;

                    // create a downloadTask for each multi-part movie.
                    foreach (var part in movieData.Parts)
                    {
                        var moviePartDownloadTask = CreateBaseTask(plexMovie);
                        moviePartDownloadTask.DownloadTaskType = DownloadTaskType.MoviePart;
                        moviePartDownloadTask.FileName = Path.GetFileName(part.File);
                        moviePartDownloadTask.DataTotal = part.Size;
                        movieDataDownloadTask.Children.Add(moviePartDownloadTask);
                    }

                    movieDataDownloadTask.DataTotal = movieDataDownloadTask.Children.Sum(x => x.DataTotal);
                    movieDownloadTask.Children.Add(movieDataDownloadTask);
                }

                movieDownloadTask.DataTotal = movieDownloadTask.Children.Sum(x => x.DataTotal);
                downloadTasks.Add(movieDownloadTask);
            }

            return Result.Ok(downloadTasks);
        }

        public Result<List<DownloadTask>> CreateDownloadTasks(List<PlexTvShow> plexTvShows)
        {
            if (!plexTvShows.Any())
            {
                return ResultExtensions.IsEmpty(nameof(plexTvShows));
            }

            static DownloadTask CreateBaseTask(PlexTvShow plexTvShow)
            {
                return new()
                {
                    MediaType = PlexMediaType.TvShow,
                    DownloadTaskType = DownloadTaskType.TvShow,
                    DownloadStatus = DownloadStatus.Initialized,
                    Created = DateTime.UtcNow,
                    Title = plexTvShow.Title,
                    Year = plexTvShow.Year,
                    PlexLibrary = plexTvShow.PlexLibrary,
                    PlexLibraryId = plexTvShow.PlexLibraryId,
                    PlexServer = plexTvShow.PlexServer,
                    PlexServerId = plexTvShow.PlexServerId,
                    Key = plexTvShow.Key,
                    Children = new List<DownloadTask>(),
                    MediaId = plexTvShow.Id,
                    DataTotal = 0,
                };
            }

            var downloadTasks = new List<DownloadTask>();
            foreach (var tvShow in plexTvShows)
            {
                var tvShowDownloadTask = CreateBaseTask(tvShow);
                var result = CreateDownloadTasks(tvShow.Seasons);
                if (result.IsFailed)
                {
                    result.LogError();
                    continue;
                }

                tvShowDownloadTask.Children = result.Value;
            }

            return Result.Ok(downloadTasks);
        }

        public Result<List<DownloadTask>> CreateDownloadTasks(List<PlexTvShowSeason> plexTvShowSeasons)
        {
            if (!plexTvShowSeasons.Any())
            {
                return ResultExtensions.IsEmpty(nameof(plexTvShowSeasons));
            }

            static DownloadTask CreateBaseTask(PlexTvShowSeason plexTvShowSeason)
            {
                return new()
                {
                    MediaType = PlexMediaType.Season,
                    DownloadTaskType = DownloadTaskType.Season,
                    DownloadStatus = DownloadStatus.Initialized,
                    Created = DateTime.UtcNow,
                    Title = plexTvShowSeason.Title,
                    Year = plexTvShowSeason.Year,
                    PlexLibrary = plexTvShowSeason.PlexLibrary,
                    PlexLibraryId = plexTvShowSeason.PlexLibraryId,
                    PlexServer = plexTvShowSeason.PlexServer,
                    PlexServerId = plexTvShowSeason.PlexServerId,
                    Key = plexTvShowSeason.Key,
                    Children = new List<DownloadTask>(),
                    MediaId = plexTvShowSeason.Id,
                    DataTotal = 0,
                };
            }

            var downloadTasks = new List<DownloadTask>();
            foreach (var season in plexTvShowSeasons)
            {
                var seasonDownloadTask = CreateBaseTask(season);
                var result = CreateDownloadTasks(season.Episodes);
                if (result.IsFailed)
                {
                    result.LogError();
                    continue;
                }

                seasonDownloadTask.Children = result.Value;

                downloadTasks.Add(seasonDownloadTask);
            }

            return Result.Ok(downloadTasks);
        }

        public Result<List<DownloadTask>> CreateDownloadTasks(List<PlexTvShowEpisode> plexTvEpisodes)
        {
            if (!plexTvEpisodes.Any())
            {
                return ResultExtensions.IsEmpty(nameof(plexTvEpisodes));
            }

            static DownloadTask CreateBaseTask(PlexTvShowEpisode plexTvShowEpisode)
            {
                return new()
                {
                    MediaType = PlexMediaType.Episode,
                    DownloadTaskType = DownloadTaskType.Episode,
                    DownloadStatus = DownloadStatus.Initialized,
                    Created = DateTime.UtcNow,
                    Title = plexTvShowEpisode.Title,
                    Year = plexTvShowEpisode.Year,
                    PlexLibrary = plexTvShowEpisode.PlexLibrary,
                    PlexLibraryId = plexTvShowEpisode.PlexLibraryId,
                    PlexServer = plexTvShowEpisode.PlexServer,
                    PlexServerId = plexTvShowEpisode.PlexServerId,
                    Key = plexTvShowEpisode.Key,
                    Children = new List<DownloadTask>(),
                    MediaId = plexTvShowEpisode.Id,
                    DataTotal = 0,
                };
            }

            var downloadTasks = new List<DownloadTask>();
            foreach (var episode in plexTvEpisodes)
            {
                var episodeDownloadTask = CreateBaseTask(episode);

                // TODO Add quality selector, now it only download the first one
                foreach (var episodePart in episode.EpisodeData.First().Parts)
                {
                    var episodePartTask = CreateBaseTask(episode);
                    episodePartTask.MediaType = PlexMediaType.Episode;
                    episodePartTask.DownloadTaskType = DownloadTaskType.EpisodePart;
                    episodePartTask.DataTotal = episodePart.Size;
                    episodeDownloadTask.Children.Add(episodePartTask);
                }

                downloadTasks.Add(episodeDownloadTask);
            }

            return Result.Ok(downloadTasks);
        }

        #endregion

        public async Task<Result<List<DownloadTask>>> GenerateDownloadTvShowTasksAsync(
            List<int> plexTvShowIds,
            List<int> plexTvShowSeasonIds,
            List<int> plexTvShowEpisodeIds)
        {
            var plexTvShows = await _mediator.Send(new GetPlexTvShowTreeByMediaIdsQuery(plexTvShowIds, plexTvShowSeasonIds, plexTvShowEpisodeIds));
            if (plexTvShows.IsFailed)
                return plexTvShows.ToResult();

            var downloadTasks = CreateDownloadTasks(plexTvShows.Value);
            if (downloadTasks.IsFailed)
                return downloadTasks.ToResult();

            var finalizeDownloadTasksResult = await FinalizeDownloadTasks(downloadTasks.Value);
            if (finalizeDownloadTasksResult.IsFailed)
                return finalizeDownloadTasksResult.ToResult().LogError();

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
            // var destinationFolder = await _folderPathService.GetDestinationFolderByMediaType(downloadTasks);
            // if (destinationFolder.IsFailed)
            //     return destinationFolder.ToResult();

            // Get plex server access token
            var serverToken = await _plexAuthenticationService.GetPlexServerTokenAsync(downloadTasks.First().PlexServerId, plexAccountId);
            if (serverToken.IsFailed)
                return serverToken.ToResult();

            var parts = _userSettings.DownloadSegments;
            foreach (var downloadTask in downloadTasks)
            {
                downloadTask.DownloadFolderId = downloadFolder.Value.Id;

                downloadTask.DownloadFolder = downloadFolder.Value;

                //   downloadTask.DestinationFolderId = destinationFolder.Value.Id;

                //   downloadTask.DestinationFolder = destinationFolder.Value;

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
                    var result = Result.Fail($"Could not recreate the download task for {downloadTask.FullTitle}");
                    result.WithReasons(mediaIdResult.Reasons);
                    await _notificationsService.SendResult(result);
                    continue;
                }

                // TODO Re-enable
                // var downloadTasksResult = await GenerateAsync(new List<int> { mediaIdResult.Value }, downloadTask.MediaType);
                // if (downloadTasksResult.IsFailed)
                // {
                //     var result = Result.Fail($"Could not recreate the download task for {downloadTask.FullTitle}");
                //     result.WithReasons(mediaIdResult.Reasons);
                //     await _notificationsService.SendResult(result);
                //     continue;
                // }
                //
                // await _mediator.Send(new DeleteDownloadWorkerTasksByDownloadTaskIdCommand(downloadTask.Id));
                //
                // //TODO Certain properties should be copied over such as priority to maintain the same order in the front-end
                // downloadTasksResult.Value[0].Id = downloadTask.Id;
                // downloadTasksResult.Value[0].Priority = downloadTask.Priority;
                // downloadTasksResult.Value[0].DownloadWorkerTasks.ForEach(x => x.DownloadTaskId = downloadTask.Id);
                //
                // freshDownloadTasks.AddRange(downloadTasksResult.Value);
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