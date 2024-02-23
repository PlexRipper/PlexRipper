using Application.Contracts;
using AutoMapper;
using Data.Contracts;
using DownloadManager.Contracts;
using FileSystem.Contracts;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;
using Settings.Contracts;

namespace PlexRipper.DownloadManager;

public class DownloadTaskFactory : IDownloadTaskFactory
{
    #region Fields

    private readonly IPathSystem _pathSystem;

    private readonly IDownloadManagerSettingsModule _downloadManagerSettings;

    private readonly IMapper _mapper;
    private readonly IPlexRipperDbContext _dbContext;

    private readonly ILog _log;
    private readonly IMediator _mediator;

    private readonly INotificationsService _notificationsService;

    #endregion

    #region Constructor

    public DownloadTaskFactory(
        ILog log,
        IMediator mediator,
        IMapper mapper,
        IPlexRipperDbContext dbContext,
        INotificationsService notificationsService,
        IPathSystem pathSystem,
        IDownloadManagerSettingsModule downloadManagerSettings)
    {
        _log = log;
        _mediator = mediator;
        _mapper = mapper;
        _dbContext = dbContext;
        _notificationsService = notificationsService;
        _pathSystem = pathSystem;
        _downloadManagerSettings = downloadManagerSettings;
    }

    #endregion

    #region Public Methods

    public async Task<Result<List<DownloadTask>>> GenerateAsync(List<DownloadMediaDTO> downloadMedias)
    {
        // TODO This might not be needed so it's disabled for now
        // var resultFolderPath = await _mediator.Send(new ValidateFolderPathsCommand());
        // if (resultFolderPath.IsFailed)
        //     return resultFolderPath.LogError();

        var downloadTasks = new List<DownloadTask>();

        var plexTvShowIds = downloadMedias.FindAll(x => x.Type == PlexMediaType.TvShow).SelectMany(x => x.MediaIds).ToList();
        var plexTvShowSeasonIds = downloadMedias.FindAll(x => x.Type == PlexMediaType.Season).SelectMany(x => x.MediaIds).ToList();
        var plexTvShowEpisodeIds = downloadMedias.FindAll(x => x.Type == PlexMediaType.Episode).SelectMany(x => x.MediaIds).ToList();
        var plexMovieIds = downloadMedias.FindAll(x => x.Type == PlexMediaType.Movie).SelectMany(x => x.MediaIds).ToList();

        if (plexTvShowIds.Any())
        {
            var result = await GenerateTvShowDownloadTasksAsync(plexTvShowIds, downloadTasks);
            if (result.IsSuccess)
                downloadTasks = result.Value;
            else
                result.LogError();
        }

        if (plexTvShowSeasonIds.Any())
        {
            var result = await GenerateTvShowSeasonDownloadTasksAsync(plexTvShowSeasonIds, downloadTasks);
            if (result.IsSuccess)
                downloadTasks = result.Value;
            else
                result.LogError();
        }

        if (plexTvShowEpisodeIds.Any())
        {
            var result = await GenerateTvShowEpisodesDownloadTasksAsync(plexTvShowEpisodeIds, downloadTasks);
            if (result.IsSuccess)
                downloadTasks = result.Value;
            else
                result.LogError();
        }

        if (plexMovieIds.Any())
        {
            var result = await GenerateMovieDownloadTasksAsync(plexMovieIds);
            if (result.IsSuccess)
                downloadTasks.AddRange(result.Value);
            else
                result.LogError();
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

    public async Task<Result<List<DownloadTask>>> GenerateTvShowDownloadTasksAsync(
        List<int> plexTvShowIds,
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

    /// <inheritdoc/>
    public async Task<Result<List<DownloadTask>>> GenerateMovieDownloadTasksAsync(List<int> plexMovieIds)
    {
        if (!plexMovieIds.Any())
            return ResultExtensions.IsEmpty(nameof(plexMovieIds)).LogWarning();

        _log.Debug("Creating {PlexMovieIdsCount} movie download tasks", plexMovieIds.Count);
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
                    moviePartDownloadTask.MediaType = PlexMediaType.Movie;
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
                movieDownloadTask.DownloadTaskType = DownloadTaskType.MovieData;
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

        _log.Debug("Regenerating {DownloadTaskIdsCount} download tasks", downloadTaskIds.Count);

        var freshDownloadTasks = new List<DownloadTask>();

        foreach (var downloadTaskId in downloadTaskIds)
        {
            var downloadTaskResult = await _mediator.Send(new GetDownloadTaskByIdQuery(downloadTaskId));
            if (downloadTaskResult.IsFailed)
                continue;

            var downloadTask = downloadTaskResult.Value;

            var mediaIdResult = await _dbContext.GetPlexMediaByMediaKeyAsync(downloadTask.Id, downloadTask.PlexServerId, downloadTask.MediaType);
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

        _log.Debug("Successfully regenerated {FreshDownloadTasksCount} out of {DownloadTaskIdsCount} download tasks", freshDownloadTasks.Count,
            downloadTaskIds.Count);

        if (downloadTaskIds.Count - freshDownloadTasks.Count > 0)
            _log.ErrorLine("Failed to generate");

        return Result.Ok(freshDownloadTasks);
    }

    public Result<List<DownloadWorkerTask>> GenerateDownloadWorkerTasks(DownloadTask downloadTask)
    {
        if (downloadTask is null)
            return ResultExtensions.IsNull(nameof(downloadTask)).LogWarning();

        var parts = _downloadManagerSettings.DownloadSegments;
        if (parts <= 0)
            return Result.Fail($"Parameter {nameof(parts)} was {parts}, prevented division by invalid value").LogWarning();

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

    #endregion

    #region Private Methods

    private async Task<Result<List<DownloadTask>>> FinalizeDownloadTasks(List<DownloadTask> downloadTasks)
    {
        if (!downloadTasks.Any())
            return ResultExtensions.IsEmpty(nameof(downloadTasks)).LogWarning();

        // Get the download folder
        var downloadFolder = await _dbContext.FolderPaths.GetDownloadFolderAsync();
        if (downloadFolder is null)
            return ResultExtensions.IsNull(nameof(downloadFolder)).LogError();

        // Get Plex libraries
        var plexLibraries = await _dbContext.PlexLibraries.Include(x => x.PlexServer).ToListAsync();
        var plexServers = plexLibraries.Select(x => x.PlexServer).DistinctBy(x => x.Id).ToList();

        // Get Folder Paths
        var folderPaths = await _dbContext.FolderPaths.ToListAsync();

        // Default destination dictionary
        var defaultDestinationDict = await _dbContext.FolderPaths.GetDefaultDestinationFolderDictionary();

        async Task<Result<List<DownloadTask>>> FillDownloadTasks(List<DownloadTask> tasks)
        {
            foreach (var downloadTask in tasks)
            {
                downloadTask.DownloadFolderId = downloadFolder.Id;
                downloadTask.DownloadFolder = downloadFolder;
                downloadTask.PlexServer = plexServers.Find(x => x.Id == downloadTask.PlexServerId);
                downloadTask.ServerMachineIdentifier = downloadTask.PlexServer.MachineIdentifier;
                var plexLibrary = plexLibraries.Find(x => x.Id == downloadTask.PlexLibraryId);
                if (plexLibrary is not null)
                {
                    if (plexLibrary.DefaultDestinationId is not null)
                    {
                        downloadTask.DestinationFolderId = plexLibrary.DefaultDestinationId ?? default(int);
                        downloadTask.DestinationFolder = folderPaths.Find(x => x.Id == downloadTask.DestinationFolderId);
                    }
                    else
                    {
                        var destination = defaultDestinationDict[downloadTask.MediaType];
                        downloadTask.DestinationFolderId = destination.Id;
                        downloadTask.DestinationFolder = destination;
                    }
                }

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
                        childTask.Parent = downloadTask;

                    var result = await FillDownloadTasks(downloadTask.Children);
                    if (result.IsFailed)
                        return result.ToResult();

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
        return GetMediaTypeDirectory(downloadTask, basePath, true);
    }

    private Result<string> GetDestinationDirectory(DownloadTask downloadTask)
    {
        if (downloadTask?.DestinationFolder is null)
            return Result.Fail("DownloadTask had an invalid DestinationFolder value");

        var basePath = downloadTask.DestinationFolder.DirectoryPath;
        return GetMediaTypeDirectory(downloadTask, basePath);
    }

    private string GetMediaTypeDirectory(DownloadTask downloadTask, string basePath, bool forDownloadFolder = false)
    {
        var downloadTaskTitle = _pathSystem.SanitizePath(downloadTask.Title);
        var titles = downloadTask.FullTitle.Split('/');

        string path;
        switch (downloadTask.MediaType)
        {
            case PlexMediaType.Movie:
                path = Path.Join($"{downloadTaskTitle} ({downloadTask.Year})");
                break;
            case PlexMediaType.TvShow:
                path = Path.Join(downloadTaskTitle);
                break;
            case PlexMediaType.Season:
                path = Path.Join(_pathSystem.SanitizePath(titles[0]), _pathSystem.SanitizePath(titles[1]));
                break;
            case PlexMediaType.Episode:
                // Since the episode can be multiple parts, we need put that in a separate folder
                path = Path.Join(_pathSystem.SanitizePath(titles[0]), _pathSystem.SanitizePath(titles[1]),
                    forDownloadFolder ? _pathSystem.SanitizePath(titles[2]) : "");
                break;
            default:
                path = Path.Join(downloadTaskTitle);
                break;
        }

        if (forDownloadFolder)
        {
            var mediaTypeFolder = downloadTask.MediaType switch
            {
                PlexMediaType.Movie => "Movies",
                PlexMediaType.TvShow => "TvShows",
                PlexMediaType.Season => "TvShows",
                PlexMediaType.Episode => "TvShows",
                _ => "Other",
            };

            return Path.Join(basePath, mediaTypeFolder, path, "/");
        }

        return Path.Join(basePath, path, "/");
    }

    #endregion
}