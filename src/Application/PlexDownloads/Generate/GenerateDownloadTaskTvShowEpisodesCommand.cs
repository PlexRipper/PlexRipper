using Application.Contracts;
using Application.Contracts.Validators;
using Data.Contracts;
using FastEndpoints;
using FluentValidation;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application;

public record GenerateDownloadTaskTvShowEpisodesCommand : ICommand<Result>
{
    public GenerateDownloadTaskTvShowEpisodesCommand(CreateDownloadTasksRequest request)
    {
        Request = request;
    }

    public GenerateDownloadTaskTvShowEpisodesCommand(List<DownloadMediaDTO> downloadMediaDtos)
    {
        Request = new CreateDownloadTasksRequest(downloadMediaDtos);
    }

    public CreateDownloadTasksRequest Request { get; }
}

public class GenerateDownloadTaskTvShowEpisodesCommandValidator
    : AbstractValidator<GenerateDownloadTaskTvShowEpisodesCommand>
{
    public GenerateDownloadTaskTvShowEpisodesCommandValidator()
    {
        RuleFor(x => x.Request.DownloadMedias).NotNull();
        RuleFor(x => x.Request.DownloadMedias).NotEmpty();
        RuleForEach(x => x.Request.DownloadMedias).SetValidator(new DownloadMediaDTOValidator());
    }
}

public class GenerateDownloadTaskTvShowEpisodesCommandHandler
    : ICommandHandler<GenerateDownloadTaskTvShowEpisodesCommand, Result>
{
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;

    private readonly List<DownloadTaskTvShow> _tvShowDownloads = [];

    public GenerateDownloadTaskTvShowEpisodesCommandHandler(ILog log, IPlexRipperDbContext dbContext)
    {
        _log = log;
        _dbContext = dbContext;
    }

    public async Task<Result> ExecuteAsync(GenerateDownloadTaskTvShowEpisodesCommand command, CancellationToken ct)
    {
        var request = command.Request;
        var groupedList = command.Request.DownloadMedias.MergeAndGroupList();
        var downloadMediaList = groupedList.FindAll(x => x.Type == PlexMediaType.Episode);
        var episodeIds = downloadMediaList.SelectMany(x => x.MediaIds).Distinct().ToList();

        if (!downloadMediaList.Any())
            return ResultExtensions.IsEmpty(nameof(downloadMediaList)).LogWarning();

        _log.Debug("Processing {PlexEpisodeIdsCount} episodes download tasks", episodeIds.Count);

        // Get all unique library IDs to optimize database queries
        var libraryIds = groupedList.Select(x => x.PlexLibraryId).Distinct().ToList();

        // Preload all required libraries into a dictionary
        var plexLibraries = await _dbContext
            .PlexLibraries.Include(x => x.PlexServer)
            .Include(x => x.DefaultDestination)
            .Where(x => libraryIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, ct);

        // Validate that all required libraries exist
        var missingLibraryIds = libraryIds.Except(plexLibraries.Keys).ToList();
        if (missingLibraryIds.Any())
        {
            _log.Warning("Missing libraries with IDs: {MissingLibraryIds}", string.Join(", ", missingLibraryIds));
            return Result.Fail($"Missing libraries with IDs: {string.Join(", ", missingLibraryIds)}").LogError();
        }

        var plexEpisodes = await _dbContext
            .PlexTvShowEpisodes.AsTracking()
            .Include(x => x.TvShow)
            .Include(x => x.TvShowSeason)
            .Include(x => x.MediaDataList)
            .ThenInclude(x => x.Parts)
            .ThenInclude(x => x.Streams)
            .Where(x => episodeIds.Contains(x.Id))
            .ToListAsync(ct);

        if (!plexEpisodes.Any())
        {
            _log.Warning("No episodes found for media IDs: {MediaIds}", string.Join(", ", episodeIds));
        }

        foreach (var tvShowEpisode in plexEpisodes)
        {
            var plexTvShow = tvShowEpisode.TvShow!;
            var plexSeason = tvShowEpisode.TvShowSeason!;

            _log.Debug(
                "Processing episode {EpisodeKey} from season {SeasonKey} of show {ShowKey}",
                tvShowEpisode.Key,
                plexSeason.Key,
                plexTvShow.Key
            );

            // Get or create Tv-show download tasks
            var downloadTaskTvShow = await GetOrCreateTvShowDownloadTaskAsync(plexTvShow, ct);
            if (downloadTaskTvShow is null)
            {
                return Result.Fail($"Failed to create or retrieve TV Show download task for {plexTvShow.Title}");
            }

            // Get or create season download task
            var downloadTaskTvShowSeason = GetOrCreateSeasonDownloadTask(plexSeason, downloadTaskTvShow);

            // Get or create episode download task
            var episodeDownloadTask = downloadTaskTvShowSeason.Children.FirstOrDefault(x => x.Key == tvShowEpisode.Key);
            if (episodeDownloadTask is null)
            {
                _log.Debug("Creating new episode download task for episode {EpisodeKey}", tvShowEpisode.Key);
                episodeDownloadTask = tvShowEpisode.MapToDownloadTask();
                episodeDownloadTask.ParentId = downloadTaskTvShowSeason.Id;
                downloadTaskTvShowSeason.Children.Add(episodeDownloadTask);
                _dbContext.DownloadTaskTvShowEpisode.Add(episodeDownloadTask);
            }
            else
            {
                _log.Debug(
                    "Found existing episode download task for episode {EpisodeKey} with ID {EpisodeId}",
                    tvShowEpisode.Key,
                    episodeDownloadTask.Id
                );
            }

            // Process episode media data
            var processResult = ProcessEpisodeMediaData(tvShowEpisode, episodeDownloadTask, request);
            if (processResult.IsFailed)
                processResult.LogError();
        }

        return (await Result.Try(() => _dbContext.SaveChangesAsync(ct))).ToResult();
    }

    private async Task<DownloadTaskTvShow?> GetOrCreateTvShowDownloadTaskAsync(
        PlexTvShow plexTvShow,
        CancellationToken ct
    )
    {
        // Check if the tvShowDownloadTask has already been created this run
        var downloadTaskTvShow = _tvShowDownloads.FirstOrDefault(x => x.Key == plexTvShow.Key);

        // Check if the tvShowDownloadTask has already been created in the database
        if (downloadTaskTvShow is null)
        {
            downloadTaskTvShow = await _dbContext.GetDownloadTaskTvShowByMediaKeyQuery(
                plexTvShow.PlexServerId,
                plexTvShow.Key,
                ct
            );
            if (downloadTaskTvShow is not null)
                _tvShowDownloads.Add(downloadTaskTvShow);
        }

        // Create a new TV Show download task if none exists
        if (downloadTaskTvShow is null)
        {
            downloadTaskTvShow = plexTvShow.MapToDownloadTask();
            _tvShowDownloads.Add(downloadTaskTvShow);
            _dbContext.DownloadTaskTvShow.Add(downloadTaskTvShow);
        }

        return downloadTaskTvShow;
    }

    private DownloadTaskTvShowSeason GetOrCreateSeasonDownloadTask(
        PlexTvShowSeason plexSeason,
        DownloadTaskTvShow downloadTaskTvShow
    )
    {
        // Check if the SeasonDownloadTask has already been created
        var downloadTaskTvShowSeason = downloadTaskTvShow.Children.FirstOrDefault(x => x.Key == plexSeason.Key);

        if (downloadTaskTvShowSeason is null)
        {
            downloadTaskTvShowSeason = plexSeason.MapToDownloadTask();
            downloadTaskTvShowSeason.ParentId = downloadTaskTvShow.Id;
            downloadTaskTvShow.Children.Add(downloadTaskTvShowSeason);
            _dbContext.DownloadTaskTvShowSeason.Add(downloadTaskTvShowSeason);
        }

        return downloadTaskTvShowSeason;
    }

    private Result ProcessEpisodeMediaData(
        PlexTvShowEpisode tvShowEpisode,
        DownloadTaskTvShowEpisode episodeDownloadTask,
        CreateDownloadTasksRequest request
    )
    {
        // TODO: Quality Selector needs to be implemented here
        var episodeData = tvShowEpisode.MediaDataList.FirstOrDefault();
        if (episodeData is null)
        {
            return ResultExtensions
                .IsEmpty(nameof(tvShowEpisode.MediaDataList))
                .WithError($"No media data found for episode {tvShowEpisode.Key} ({tvShowEpisode.Title})")
                .LogError();
        }

        // Map episodeData to DownloadTaskTvShowEpisodeFile and add to episodeDownloadTask
        var downloadFiles = episodeData.MapToDownloadTask(tvShowEpisode, request);

        if (!downloadFiles.Any())
        {
            return Result.Fail($"No download files generated for episode {tvShowEpisode.Key} ({tvShowEpisode.Title})");
        }

        episodeDownloadTask.Children.AddRange(downloadFiles);
        _dbContext.DownloadTaskTvShowEpisodeFile.AddRange(downloadFiles);

        return Result.Ok();
    }
}
