using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FluentResults;
using Logging;
using MediatR;
using PlexRipper.Application;
using PlexRipper.Domain;

namespace PlexRipper.DownloadManager
{
    public class DownloadTaskFactory : IDownloadTaskFactory
    {
        #region Fields

        private readonly IFolderPathService _folderPathService;

        private readonly IMapper _mapper;

        private readonly IMediator _mediator;

        private readonly INotificationsService _notificationsService;

        private readonly IPlexAuthenticationService _plexAuthenticationService;

        #endregion

        #region Constructor

        public DownloadTaskFactory(
            IMediator mediator,
            IMapper mapper,
            IPlexAuthenticationService plexAuthenticationService,
            INotificationsService notificationsService,
            IFolderPathService folderPathService)
        {
            _mediator = mediator;
            _mapper = mapper;
            _plexAuthenticationService = plexAuthenticationService;
            _notificationsService = notificationsService;
            _folderPathService = folderPathService;
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

            if (plexTvShowIds.Any())
            {
                var result = await GenerateTvShowDownloadTasksAsync(plexTvShowIds, downloadTasks);
                if (result.IsSuccess)
                {
                    downloadTasks = result.Value;
                }
                else
                {
                    result.LogError();
                }
            }

            if (plexTvShowSeasonIds.Any())
            {
                var result = await GenerateTvShowSeasonDownloadTasksAsync(plexTvShowSeasonIds, downloadTasks);
                if (result.IsSuccess)
                {
                    downloadTasks = result.Value;
                }
                else
                {
                    result.LogError();
                }
            }

            if (plexTvShowEpisodeIds.Any())
            {
                var result = await GenerateTvShowEpisodesDownloadTasksAsync(plexTvShowEpisodeIds, downloadTasks);
                if (result.IsSuccess)
                {
                    downloadTasks = result.Value;
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

        private DownloadTask CreateEpisodeDownloadTask(PlexTvShowEpisode episode)
        {
            var episodeDownloadTask = _mapper.Map<DownloadTask>(episode);

            // TODO Takes first entry which assumes its the highest quality one, should be configurable
            var episodeData = episode.EpisodeData.First();

            if (episodeData.IsMultiPart)
            {
                foreach (var episodePart in episodeData.Parts)
                {
                    var episodePartTask = _mapper.Map<DownloadTask>(episode);
                    episodePartTask.MediaType = PlexMediaType.Episode;
                    episodePartTask.DownloadTaskType = DownloadTaskType.EpisodePart;
                    episodePartTask.DataTotal = episodePart.Size;
                    episodePartTask.FileName = Path.GetFileName(episodePart.File);
                    episodePartTask.FileLocationUrl = episodePart.ObfuscatedFilePath;
                    episodeDownloadTask.Children.Add(episodePartTask);
                }

                episodeDownloadTask.DataTotal = episodeDownloadTask.Children.Select(x => x.DataTotal).Sum();
            }
            else
            {
                var part = episodeData.Parts.First();
                episodeDownloadTask.MediaType = PlexMediaType.Episode;
                episodeDownloadTask.DownloadTaskType = DownloadTaskType.Episode;
                episodeDownloadTask.DataTotal = part.Size;
                episodeDownloadTask.FileName = Path.GetFileName(part.File);
                episodeDownloadTask.FileLocationUrl = part.ObfuscatedFilePath;
            }

            return episodeDownloadTask;
        }

        public async Task<Result<List<DownloadTask>>> GenerateTvShowDownloadTasksAsync(List<int> plexTvShowIds,
            List<DownloadTask> downloadTasks = null)
        {
            if (!plexTvShowIds.Any())
                return ResultExtensions.IsEmpty(nameof(plexTvShowIds)).LogWarning();

            downloadTasks ??= new List<DownloadTask>();

            foreach (var tvShowId in plexTvShowIds)
            {
                var tvShowResult = await _mediator.Send(new GetPlexTvShowByIdWithEpisodesQuery(tvShowId, true, true));
                if (tvShowResult.IsFailed)
                {
                    tvShowResult.LogError();
                    continue;
                }

                var tvShow = tvShowResult.Value;

                // Check if the tvShowDownloadTask has already been created
                var tvShowDownloadTaskIndex = downloadTasks.FindIndex(x => x.Equals(tvShow));
                if (tvShowDownloadTaskIndex == -1)
                {
                    var result = await _mediator.Send(new GetDownloadTaskByMediaKeyQuery(tvShow.PlexServerId, tvShow.Key));
                    var tvShowDownloadTask = result.IsSuccess ? result.Value : _mapper.Map<DownloadTask>(tvShow);
                    downloadTasks.Add(tvShowDownloadTask);
                }

                // We set this to not have to query the database to deep
                tvShow.Seasons.ForEach(x =>
                {
                    x.TvShow = tvShow;
                    x.PlexServer = tvShow.PlexServer;
                    x.PlexLibrary = tvShow.PlexLibrary;
                });

                // Create seasons downloadTasks
                var seasonIds = tvShow.Seasons.Select(x => x.Id).ToList();
                var seasonsResult = await GenerateTvShowSeasonDownloadTasksAsync(seasonIds, downloadTasks, tvShow.Seasons);
                if (seasonsResult.IsFailed)
                {
                    seasonsResult.LogError();
                    continue;
                }

                downloadTasks = seasonsResult.Value;
            }

            downloadTasks.ForEach(x => x.Calculate());

            return Result.Ok(downloadTasks);
        }

        public async Task<Result<List<DownloadTask>>> GenerateTvShowSeasonDownloadTasksAsync(
            List<int> plexTvShowSeasonIds,
            List<DownloadTask> downloadTasks = null,
            List<PlexTvShowSeason> seasons = null)
        {
            if (!plexTvShowSeasonIds.Any())
                return ResultExtensions.IsEmpty(nameof(plexTvShowSeasonIds)).LogWarning();

            downloadTasks ??= new List<DownloadTask>();

            foreach (var seasonId in plexTvShowSeasonIds)
            {
                var season = seasons?.Find(x => x.Id == seasonId);
                if (season is null)
                {
                    var seasonResult = await _mediator.Send(new GetPlexTvShowSeasonByIdQuery(seasonId));
                    if (seasonResult.IsFailed)
                    {
                        seasonResult.LogError();
                        continue;
                    }

                    season = seasonResult.Value;
                }

                // Check if the tvShowDownloadTask has already been created
                var tvShowDownloadTaskIndex = downloadTasks.FindIndex(x => x.Equals(season.TvShow));
                if (tvShowDownloadTaskIndex == -1)
                {
                    var result = await _mediator.Send(new GetDownloadTaskByMediaKeyQuery(season.TvShow.PlexServerId, season.TvShow.Key));
                    downloadTasks.Add(result.IsSuccess ? result.Value : _mapper.Map<DownloadTask>(season.TvShow));
                    tvShowDownloadTaskIndex = downloadTasks.FindIndex(x => x.Equals(season.TvShow));
                }

                // Check if the tvShowSeasonDownloadTask has already been created
                var seasonDownloadTaskIndex = downloadTasks[tvShowDownloadTaskIndex].Children.FindIndex(x => x.Equals(season));
                if (seasonDownloadTaskIndex == -1)
                {
                    var result = await _mediator.Send(new GetDownloadTaskByMediaKeyQuery(season.PlexServerId, season.Key));
                    var seasonDownloadTask = result.IsSuccess ? result.Value : _mapper.Map<DownloadTask>(season);
                    seasonDownloadTask.ParentId = downloadTasks[tvShowDownloadTaskIndex].Id;
                    downloadTasks[tvShowDownloadTaskIndex].Children.Add(seasonDownloadTask);
                }

                // We set this to not have to query the database to deep
                season.Episodes.ForEach(x =>
                {
                    x.TvShow = season.TvShow;
                    x.TvShowSeason = season;
                    x.PlexServer = season.PlexServer;
                    x.PlexLibrary = season.PlexLibrary;
                });
                // Create episodes downloadTasks
                var episodesIds = season.Episodes.Select(x => x.Id).ToList();
                var seasonsResult = await GenerateTvShowEpisodesDownloadTasksAsync(episodesIds, downloadTasks, season.Episodes);
                if (seasonsResult.IsFailed)
                {
                    seasonsResult.LogError();
                    continue;
                }

                downloadTasks = seasonsResult.Value;
            }

            downloadTasks.ForEach(x => x.Calculate());

            return Result.Ok(downloadTasks);
        }

        public async Task<Result<List<DownloadTask>>> GenerateTvShowEpisodesDownloadTasksAsync(
            List<int> plexTvShowEpisodeIds,
            List<DownloadTask> downloadTasks = null,
            List<PlexTvShowEpisode> episodes = null)
        {
            if (!plexTvShowEpisodeIds.Any())
                return ResultExtensions.IsEmpty(nameof(plexTvShowEpisodeIds)).LogWarning();

            downloadTasks ??= new List<DownloadTask>();

            foreach (var episodeId in plexTvShowEpisodeIds)
            {
                // Retrieve episode media from database or passed in list
                var episode = episodes?.Find(x => x.Id == episodeId);
                if (episode is null)
                {
                    var episodeResult = await _mediator.Send(new GetPlexTvShowEpisodeByIdQuery(episodeId));
                    if (episodeResult.IsFailed)
                    {
                        episodeResult.LogError();
                        continue;
                    }

                    episode = episodeResult.Value;
                }

                // Check if the tvShowDownloadTask has already been created
                var tvShowDownloadTaskIndex = downloadTasks.FindIndex(x => x.Equals(episode.TvShow));
                if (tvShowDownloadTaskIndex == -1)
                {
                    var result = await _mediator.Send(new GetDownloadTaskByMediaKeyQuery(episode.PlexServerId, episode.TvShow.Key));
                    downloadTasks.Add(result.IsSuccess ? result.Value : _mapper.Map<DownloadTask>(episode.TvShow));
                    tvShowDownloadTaskIndex = downloadTasks.FindIndex(x => x.Equals(episode.TvShow));
                }

                // Check if the tvShowSeasonDownloadTask has already been created
                var seasonDownloadTaskIndex = downloadTasks[tvShowDownloadTaskIndex].Children.FindIndex(x => x.Equals(episode.TvShowSeason));
                if (seasonDownloadTaskIndex == -1)
                {
                    var result = await _mediator.Send(new GetDownloadTaskByMediaKeyQuery(episode.TvShowSeason.PlexServerId,
                        episode.TvShowSeason.Key));
                    var seasonDownloadTask = result.IsSuccess ? result.Value : _mapper.Map<DownloadTask>(episode.TvShowSeason);
                    seasonDownloadTask.ParentId = downloadTasks[tvShowDownloadTaskIndex].Id;
                    downloadTasks[tvShowDownloadTaskIndex].Children.Add(seasonDownloadTask);
                    seasonDownloadTaskIndex = downloadTasks[tvShowDownloadTaskIndex].Children.FindIndex(x => x.Equals(episode.TvShowSeason));
                }

                // Check if the tvShowEpisodesDownloadTask has already been created
                var episodeDownloadTaskIndex = downloadTasks[tvShowDownloadTaskIndex]
                    .Children[seasonDownloadTaskIndex]
                    .Children.FindIndex(x => x.Equals(episode));
                if (episodeDownloadTaskIndex == -1)
                {
                    var result = await _mediator.Send(new GetDownloadTaskByMediaKeyQuery(episode.PlexServerId, episode.Key));
                    var episodeDownloadTask = result.IsSuccess ? result.Value : CreateEpisodeDownloadTask(episode);
                    episodeDownloadTask.ParentId = downloadTasks[tvShowDownloadTaskIndex].Children[seasonDownloadTaskIndex].Id;
                    downloadTasks[tvShowDownloadTaskIndex].Children[seasonDownloadTaskIndex].Children.Add(episodeDownloadTask);
                }
            }

            downloadTasks.ForEach(x => x.Calculate());

            return Result.Ok(downloadTasks);
        }

        /// <summary>
        /// Creates <see cref="DownloadTask"/>s from a <see cref="PlexMovie"/> and send it to the <see cref="IDownloadManager"/>.
        /// </summary>
        /// <param name="plexMovieIds">The ids of the <see cref="PlexMovie"/> to create <see cref="DownloadTask"/>s from.</param>
        /// <returns>The created <see cref="DownloadTask"/>.</returns>
        public async Task<Result<List<DownloadTask>>> GenerateMovieDownloadTasksAsync(List<int> plexMovieIds)
        {
            if (!plexMovieIds.Any())
                return ResultExtensions.IsEmpty(nameof(plexMovieIds)).LogWarning();

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

                // TODO Takes first entry which assumes its the highest quality one
                var movieData = plexMovie.MovieData.First();

                if (movieData.IsMultiPart)
                {
                    // create a downloadTask for each multi-part movie.
                    foreach (var part in movieData.Parts)
                    {
                        var moviePartDownloadTask = _mapper.Map<DownloadTask>(plexMovie);
                        moviePartDownloadTask.FullTitle = fullTitle;
                        moviePartDownloadTask.DownloadTaskType = DownloadTaskType.MoviePart;
                        moviePartDownloadTask.FileName = Path.GetFileName(part.File);
                        moviePartDownloadTask.DataTotal = part.Size;
                        moviePartDownloadTask.Quality = movieData.VideoResolution;
                        moviePartDownloadTask.FileLocationUrl = part.ObfuscatedFilePath;
                        moviePartDownloadTask.FileName = Path.GetFileName(part.File);
                        movieDownloadTask.Children.Add(moviePartDownloadTask);
                    }

                    // Calculate total data
                    movieDownloadTask.DataTotal = movieDownloadTask.Children.Select(x => x.DataTotal).Sum();
                }
                else
                {
                    var part = movieData.Parts.First();
                    movieDownloadTask.MediaType = PlexMediaType.Movie;
                    movieDownloadTask.DownloadTaskType = DownloadTaskType.Movie;
                    movieDownloadTask.DataTotal = part.Size;
                    movieDownloadTask.FileName = Path.GetFileName(part.File);
                    movieDownloadTask.FileLocationUrl = part.ObfuscatedFilePath;
                }

                downloadTasks.Add(movieDownloadTask);
            }

            return Result.Ok(downloadTasks);
        }

        /// <inheritdoc/>
        public async Task<Result<List<DownloadTask>>> RegenerateDownloadTask(List<int> downloadTaskIds)
        {
            if (!downloadTaskIds.Any())
                return ResultExtensions.IsEmpty(nameof(downloadTaskIds)).LogWarning();

            Log.Debug($"Regenerating {downloadTaskIds.Count} download tasks.");

            var freshDownloadTasks = new List<DownloadTask>();

            foreach (var downloadTaskId in downloadTaskIds)
            {
                var downloadTaskResult = await _mediator.Send(new GetDownloadTaskByIdQuery(downloadTaskId));
                if (downloadTaskResult.IsFailed)
                {
                    continue;
                }

                var downloadTask = downloadTaskResult.Value;

                var mediaIdResult = await _mediator.Send(new GetPlexMediaIdByKeyQuery(downloadTask));
                if (mediaIdResult.IsFailed)
                {
                    var result = Result.Fail($"Could not recreate the download task for {downloadTask.FullTitle}");
                    result.WithReasons(mediaIdResult.Reasons);
                    await _notificationsService.SendResult(result);
                    continue;
                }

                var list = new List<DownloadMediaDTO>
                {
                    new()
                    {
                        Type = downloadTask.MediaType,
                        MediaIds = new List<int> { mediaIdResult.Value },
                    },
                };

                var downloadTasksResult = await GenerateAsync(list);
                if (downloadTasksResult.IsFailed)
                {
                    var result = Result.Fail($"Could not recreate the download task for {downloadTask.FullTitle}").WithReasons(mediaIdResult.Reasons);
                    await _notificationsService.SendResult(result);
                    continue;
                }

                await _mediator.Send(new DeleteDownloadWorkerTasksByDownloadTaskIdCommand(downloadTask.Id));

                downloadTasksResult.Value[0].Id = downloadTask.Id;
                downloadTasksResult.Value[0].Priority = downloadTask.Priority;

                freshDownloadTasks.AddRange(downloadTasksResult.Value);
            }

            Log.Debug($"Successfully regenerated {freshDownloadTasks.Count} out of {downloadTaskIds.Count} download tasks.");
            if (downloadTaskIds.Count - freshDownloadTasks.Count > 0)
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

            // Get the download folder
            var downloadFolder = await _folderPathService.GetDownloadFolderAsync();
            if (downloadFolder.IsFailed)
                return downloadFolder.ToResult();

            // Get Plex libraries
            var plexLibraries = await _mediator.Send(new GetAllPlexLibrariesQuery(true));
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

            async Task<Result<List<DownloadTask>>> FillDownloadTasks(List<DownloadTask> tasks)
            {
                foreach (var downloadTask in tasks)
                {
                    downloadTask.DownloadFolderId = downloadFolder.Value.Id;
                    downloadTask.DownloadFolder = downloadFolder.Value;

                    var plexLibrary = plexLibraries.Value.Find(x => x.Id == downloadTask.PlexLibraryId);
                    if (plexLibrary is not null)
                    {
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

                    // Create Download URL
                    var downloadUrl = $"{downloadTask.PlexServer.ServerUrl}{downloadTask.FileLocationUrl}";
                    var serverTokenWithUrl = await _plexAuthenticationService.GetPlexServerTokenWithUrl(downloadTask.PlexServerId, downloadUrl);
                    if (serverTokenWithUrl.IsFailed)
                    {
                        Log.Error($"Failed to retrieve server token to create DownloadUrl for PlexServer {downloadTask.PlexServer.Name}");
                        return serverTokenWithUrl.ToResult();
                    }

                    downloadTask.DownloadUrl = serverTokenWithUrl.Value;

                    // Determine download directory
                    var downloadDir = GetDownloadDirectory(downloadTask);
                    if (downloadDir.IsFailed)
                        return downloadDir.ToResult();

                    downloadTask.DownloadDirectory = downloadDir.Value;

                    // Determine destination directory
                    var destinationDir = GetDestinationDirectory(downloadTask);
                    if (destinationDir.IsFailed)
                        return destinationDir.ToResult();

                    downloadTask.DestinationDirectory = destinationDir.Value;

                    if (downloadTask.Children.Any())
                    {
                        foreach (var childTask in downloadTask.Children)
                        {
                            childTask.Parent = downloadTask;
                        }

                        var result = await FillDownloadTasks(downloadTask.Children);
                        if (result.IsFailed)
                        {
                            return result.ToResult();
                        }

                        downloadTask.Children = result.Value;
                    }
                }

                return Result.Ok(tasks);
            }

            return await FillDownloadTasks(downloadTasks);
        }

        private Result<string> GetDownloadDirectory(DownloadTask downloadTask)
        {
            if (downloadTask?.DownloadFolder is null)
                return Result.Fail("DownloadTask had an invalid DownloadFolder value");

            var basePath = downloadTask.DownloadFolder.DirectoryPath;

            var parent = downloadTask.Parent;
            switch (downloadTask.MediaType)
            {
                case PlexMediaType.Movie:
                    return Result.Ok(Path.Join(basePath, "Movies", $"{downloadTask.Title} ({downloadTask.Year})"));
                case PlexMediaType.TvShow:
                    return Result.Ok(Path.Join(basePath, "TvShows", $"{downloadTask.Title} ({downloadTask.Year})"));
                case PlexMediaType.Season:
                    return Result.Ok(Path.Join(basePath, "TvShows", $"{parent.Title} ({parent.Year})", downloadTask.Title));
                case PlexMediaType.Episode:
                    var grandParent = downloadTask.Parent?.Parent;
                    return Result.Ok(Path.Join(basePath, "TvShows", $"{grandParent.Title} ({grandParent.Year})", parent.Title));
                default:
                    return Result.Ok(Path.Join(basePath, "Other", $"{downloadTask.Title} ({downloadTask.Year})"));
            }
        }

        private Result<string> GetDestinationDirectory(DownloadTask downloadTask)
        {
            if (downloadTask?.DestinationFolder is null)
                return Result.Fail("DownloadTask had an invalid DestinationFolder value");

            var basePath = downloadTask.DestinationFolder.DirectoryPath;

            var parent = downloadTask.Parent;
            switch (downloadTask.MediaType)
            {
                case PlexMediaType.Movie:
                    return Result.Ok(Path.Join(basePath, "Movies", $"{downloadTask.Title} ({downloadTask.Year})"));
                case PlexMediaType.TvShow:
                    return Result.Ok(Path.Join(basePath, "TvShows", $"{downloadTask.Title} ({downloadTask.Year})"));
                case PlexMediaType.Season:
                    return Result.Ok(Path.Join(basePath, "TvShows", $"{parent.Title} ({parent.Year})", downloadTask.Title));
                case PlexMediaType.Episode:
                    var grandParent = downloadTask.Parent?.Parent;
                    return Result.Ok(Path.Join(basePath, "TvShows", $"{grandParent.Title} ({grandParent.Year})", parent.Title));
                default:
                    return Result.Ok(Path.Join(basePath, "Other", $"{downloadTask.Title} ({downloadTask.Year})"));
            }
        }

        #endregion
    }
}