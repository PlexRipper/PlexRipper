using Application.Contracts;
using Application.Contracts.Validators;
using Data.Contracts;
using FluentValidation;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application;

public record GenerateDownloadTaskTvShowEpisodesCommand : IRequest<Result>
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
    : IRequestHandler<GenerateDownloadTaskTvShowEpisodesCommand, Result>
{
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;

    public GenerateDownloadTaskTvShowEpisodesCommandHandler(ILog log, IPlexRipperDbContext dbContext)
    {
        _log = log;
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(
        GenerateDownloadTaskTvShowEpisodesCommand command,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var groupedList = command.Request.DownloadMedias.MergeAndGroupList();
            var request = command.Request;
            var plexEpisodeList = groupedList.FindAll(x => x.Type == PlexMediaType.Episode);
            if (!plexEpisodeList.Any())
                return ResultExtensions.IsEmpty(nameof(plexEpisodeList)).LogWarning();

            _log.Debug(
                "Creating {PlexEpisodeIdsCount} episodes download tasks",
                plexEpisodeList.SelectMany(x => x.MediaIds).ToList().Count
            );

            foreach (var downloadMediaDto in plexEpisodeList)
            {
                // TODO optimize this query and run it outside the loop
                await _dbContext
                    .PlexLibraries.Include(x => x.PlexServer)
                    .Include(x => x.DefaultDestination)
                    .GetAsync(downloadMediaDto.PlexLibraryId, cancellationToken);

                var plexEpisodes = await _dbContext
                    .PlexTvShowEpisodes.AsTracking()
                    .IncludeAll()
                    .Where(x => downloadMediaDto.MediaIds.Contains(x.Id))
                    .ToListAsync(cancellationToken);

                var tvShowDownloads = new List<DownloadTaskTvShow>();

                foreach (var tvShowEpisode in plexEpisodes)
                {
                    var plexTvShow = tvShowEpisode.TvShow;
                    var plexSeason = tvShowEpisode.TvShowSeason;

                    if (plexTvShow is null)
                    {
                        _log.Here()
                            .Warning(
                                "PlexTvShow is null for episode {EpisodeKey} ({EpisodeTitle})",
                                tvShowEpisode.Key,
                                tvShowEpisode.Title
                            );
                        continue;
                    }

                    if (plexSeason is null)
                    {
                        _log.Here()
                            .Warning(
                                "PlexTvShowSeason is null for episode {EpisodeKey} ({EpisodeTitle})",
                                tvShowEpisode.Key,
                                tvShowEpisode.Title
                            );
                        continue;
                    }

                    // Check if the tvShowDownloadTask has already been created this run
                    var downloadTaskTvShow = tvShowDownloads.FirstOrDefault(x => x.Key == plexTvShow.Key);

                    // Check if the tvShowDownloadTask has already been created ever
                    if (downloadTaskTvShow is null)
                    {
                        downloadTaskTvShow = await _dbContext.GetDownloadTaskTvShowByMediaKeyQuery(
                            plexTvShow.PlexServerId,
                            plexTvShow.Key,
                            cancellationToken
                        );
                        if (downloadTaskTvShow is not null)
                            tvShowDownloads.Add(downloadTaskTvShow);
                    }

                    if (downloadTaskTvShow is null)
                    {
                        downloadTaskTvShow = plexTvShow.MapToDownloadTask();
                        tvShowDownloads.Add(downloadTaskTvShow);
                        _dbContext.DownloadTaskTvShow.Add(downloadTaskTvShow);
                    }

                    // Check if the SeasonDownloadTask has already been created
                    var downloadTaskTvShowSeason = downloadTaskTvShow.Children.FirstOrDefault(x =>
                        x.Key == plexSeason.Key
                    );
                    if (downloadTaskTvShowSeason is null)
                    {
                        downloadTaskTvShowSeason = plexSeason.MapToDownloadTask();
                        downloadTaskTvShowSeason.ParentId = downloadTaskTvShow.Id;
                        downloadTaskTvShow.Children.Add(downloadTaskTvShowSeason);
                        _dbContext.DownloadTaskTvShowSeason.Add(downloadTaskTvShowSeason);
                    }

                    // Check if the tvShowEpisodesDownloadTask has already been created
                    var episodeDownloadTask = downloadTaskTvShowSeason.Children.FirstOrDefault(x =>
                        x.Key == plexSeason.Key
                    );
                    if (episodeDownloadTask is null)
                    {
                        episodeDownloadTask = tvShowEpisode.MapToDownloadTask();
                        episodeDownloadTask.ParentId = downloadTaskTvShowSeason.Id;
                        downloadTaskTvShowSeason.Children.Add(episodeDownloadTask);
                        _dbContext.DownloadTaskTvShowEpisode.Add(episodeDownloadTask);
                    }

                    // TODO: Quality Selector needs to be implemented here
                    var episodeData = tvShowEpisode.MediaDataList.FirstOrDefault();
                    if (episodeData is null)
                    {
                        _log.Here()
                            .Warning(
                                "No media data found for episode {EpisodeKey} ({EpisodeTitle})",
                                tvShowEpisode.Key,
                                tvShowEpisode.Title
                            );
                        continue;
                    }

                    // Map movieData to DownloadTaskMovieFile and add to movieDownloadTask
                    var downloadFiles = episodeData.MapToDownloadTask(tvShowEpisode, request);
                    episodeDownloadTask.Children.AddRange(downloadFiles);
                    _dbContext.DownloadTaskTvShowEpisodeFile.AddRange(downloadFiles);
                }
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new ExceptionalError(ex)).LogError();
        }
    }
}
