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
using PlexRipper.Application.FolderPaths;
using PlexRipper.Application.PlexLibraries;
using PlexRipper.Application.PlexMedia;
using PlexRipper.Application.PlexMovies;
using PlexRipper.Application.PlexTvShows;
using PlexRipper.Domain;

namespace PlexRipper.Application
{
    public class PlexDownloadTaskFactory : IPlexDownloadTaskFactory
    {
        #region Fields

        private readonly IFolderPathService _folderPathService;

        private readonly IMapper _mapper;

        private readonly IMediator _mediator;

        private readonly INotificationsService _notificationsService;

        private readonly IPlexAuthenticationService _plexAuthenticationService;

        private readonly IUserSettings _userSettings;

        #endregion

        #region Constructor

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

        #endregion

        #region Public Methods

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

            // Add the final property values
            var finalizeDownloadTasksResult = await FinalizeDownloadTasks(downloadTasks);
            if (finalizeDownloadTasksResult.IsFailed)
                return finalizeDownloadTasksResult.ToResult().LogError();

            return Result.Ok(finalizeDownloadTasksResult.Value);
        }

        public async Task<Result<List<DownloadTask>>> GenerateDownloadTvShowTasksAsync(
            List<int> plexTvShowIds,
            List<int> plexTvShowSeasonIds,
            List<int> plexTvShowEpisodeIds)
        {
            var plexTvShows = await _mediator.Send(new GetPlexTvShowTreeByMediaIdsQuery(plexTvShowIds, plexTvShowSeasonIds, plexTvShowEpisodeIds));
            if (plexTvShows.IsFailed)
                return plexTvShows.ToResult();

            // Create download tasks
            var downloadTasks = new List<DownloadTask>();
            foreach (var tvShow in plexTvShows.Value)
            {
                var tvShowDownloadTask = _mapper.Map<DownloadTask>(tvShow);

                foreach (var season in tvShow.Seasons)
                {
                    var seasonDownloadTask = _mapper.Map<DownloadTask>(season);

                    foreach (var episode in season.Episodes)
                    {
                        var episodeDownloadTask = _mapper.Map<DownloadTask>(episode);

                        foreach (var mediaData in episode.EpisodeData)
                        {
                            // TODO Add quality selector
                            foreach (var episodePart in mediaData.Parts)
                            {
                                var episodePartTask = _mapper.Map<DownloadTask>(episode);
                                episodePartTask.MediaType = PlexMediaType.Episode;
                                episodePartTask.DownloadTaskType =
                                    mediaData.Parts.Count == 1 ? DownloadTaskType.EpisodeData : DownloadTaskType.EpisodePart;
                                episodePartTask.DataTotal = episodePart.Size;
                                episodePartTask.FileName = episodePart.File;
                                episodePartTask.FileLocationUrl = episodePart.ObfuscatedFilePath;
                                episodeDownloadTask.Children.Add(episodePartTask);
                            }
                        }

                        seasonDownloadTask.Children.Add(episodeDownloadTask);
                    }

                    tvShowDownloadTask.Children.Add(seasonDownloadTask);
                }

                downloadTasks.Add(tvShowDownloadTask);
            }

            return Result.Ok(downloadTasks);
        }

        public Result<List<DownloadWorkerTask>> GenerateDownloadWorkerTasks(DownloadTask downloadTask, int parts)
        {
            if (downloadTask is null)
            {
                return ResultExtensions.IsNull(nameof(downloadTask));
            }

            if (parts <= 0)
            {
                return Result.Fail($"Parameter {nameof(parts)} was {parts}, prevented division by invalid value").LogWarning();
            }

            // Create download worker tasks/segments/ranges
            var totalBytesToReceive = downloadTask.DataTotal;
            var partSize = totalBytesToReceive / parts;
            var remainder = totalBytesToReceive - partSize * parts;

            var downloadWorkerTasks = new List<DownloadWorkerTask>();

            for (var i = 0; i < parts; i++)
            {
                var start = partSize * i;
                var end = start + partSize;
                if (i == parts - 1 && remainder > 0)
                {
                    // Add the remainder to the last download range
                    end += remainder;
                }

                downloadWorkerTasks.Add(new DownloadWorkerTask(downloadTask, i + 1, start, end));
            }

            return Result.Ok(downloadWorkerTasks);
        }

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

            // Create downloadTasks
            var downloadTasks = new List<DownloadTask>();
            foreach (var plexMovie in plexMoviesResult.Value)
            {
                var movieDownloadTask = _mapper.Map<DownloadTask>(plexMovie);
                var fullTitle = $"{plexMovie.Title} ({plexMovie.Year})";
                movieDownloadTask.FullTitle = fullTitle;

                // TODO Add quality chooser, will download everything now
                foreach (var mediaData in plexMovie.MovieData)
                {
                    // create a downloadTask for each multi-part movie.
                    foreach (var part in mediaData.Parts)
                    {
                        var moviePartDownloadTask = _mapper.Map<DownloadTask>(plexMovie);
                        moviePartDownloadTask.FullTitle = fullTitle;
                        moviePartDownloadTask.DownloadTaskType = mediaData.Parts.Count == 1 ? DownloadTaskType.MovieData : DownloadTaskType.MoviePart;
                        moviePartDownloadTask.FileName = Path.GetFileName(part.File);
                        moviePartDownloadTask.DataTotal = part.Size;
                        moviePartDownloadTask.Quality = mediaData.VideoResolution;
                        moviePartDownloadTask.FileLocationUrl = part.ObfuscatedFilePath;
                        moviePartDownloadTask.FileName = Path.GetFileName(part.File);
                        movieDownloadTask.Children.Add(moviePartDownloadTask);
                    }
                }

                // Calculate total data
                movieDownloadTask.DataTotal = movieDownloadTask.Children.Sum(x => x.DataTotal);
                downloadTasks.Add(movieDownloadTask);
            }

            return Result.Ok(downloadTasks);
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

        #region Private Methods

        private async Task<Result<List<DownloadTask>>> FinalizeDownloadTasks(List<DownloadTask> downloadTasks)
        {
            if (!downloadTasks.Any())
                return ResultExtensions.IsEmpty(nameof(downloadTasks)).LogWarning();

            var parts = _userSettings.DownloadSegments;
            if (parts <= 0)
                return Result.Fail($"The DownloadSegments value has an invalid value of {parts}");

            // Get the download folder
            var downloadFolder = await _folderPathService.GetDownloadFolderAsync();
            if (downloadFolder.IsFailed)
                return downloadFolder.ToResult();

            // Get Plex libraries
            var plexLibraries = await _mediator.Send(new GetAllPlexLibrariesQuery());
            if (plexLibraries.IsFailed)
                return plexLibraries.ToResult();

            // Get Plex libraries
            var folderPaths = await _mediator.Send(new GetAllFolderPathsQuery());
            if (folderPaths.IsFailed)
                return folderPaths.ToResult();

            // Default destination dictionary
            var defaultDestinationDict = await _folderPathService.GetDefaultDestinationFolderDictionary();
            if (defaultDestinationDict.IsFailed)
                return defaultDestinationDict.ToResult();

            Result<List<DownloadTask>> FillDownloadTasks(List<DownloadTask> tasks)
            {
                foreach (var downloadTask in tasks)
                {
                    downloadTask.DownloadFolderId = downloadFolder.Value.Id;
                    downloadTask.DownloadFolder = downloadFolder.Value;

                    var plexLibrary = plexLibraries.Value.Find(x => x.Id == downloadTask.PlexLibraryId);
                    if (plexLibrary is not null)
                    {
                        downloadTask.PlexLibrary = plexLibrary;

                        if (plexLibrary.DefaultDestinationId is not null)
                        {
                            downloadTask.DestinationFolderId = plexLibrary.DefaultDestinationId ?? default(int);
                            downloadTask.DestinationFolder = folderPaths.Value.Find(x => x.Id == downloadTask.DestinationFolderId);
                        }
                        else
                        {
                            var destination = defaultDestinationDict.Value[downloadTask.MediaType];
                            downloadTask.DestinationFolderId = destination.Id;
                            downloadTask.DestinationFolder = destination;
                        }
                    }

                    // downloadTask.ServerToken = serverToken.Value;
                    if (downloadTask.MediaType is PlexMediaType.Episode or PlexMediaType.Movie)
                    {
                        var downloadWorkerTasks = GenerateDownloadWorkerTasks(downloadTask, parts);
                        if (downloadWorkerTasks.IsFailed)
                        {
                            return downloadWorkerTasks.ToResult();
                        }

                        downloadTask.DownloadWorkerTasks = downloadWorkerTasks.Value;
                    }

                    if (downloadTask.Children.Any())
                    {
                        var result = FillDownloadTasks(downloadTask.Children);
                        if (result.IsFailed)
                        {
                            return result.ToResult();
                        }

                        downloadTask.Children = result.Value;
                    }
                }

                return Result.Ok(tasks);
            }

            return FillDownloadTasks(downloadTasks);
        }

        #endregion
    }
}