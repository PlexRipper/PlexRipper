using Data.Contracts;
using DownloadManager.Contracts;
using DownloadManager.Contracts.Extensions;
using FluentValidation;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Domain.PlexMediaExtensions;

namespace PlexRipper.Application;

public record GenerateDownloadTaskTvShowSeasonsCommand(List<DownloadMediaDTO> DownloadMedias) : IRequest<Result>;

public class GenerateDownloadTaskTvShowSeasonsCommandValidator : AbstractValidator<GenerateDownloadTaskTvShowSeasonsCommand>
{
    public GenerateDownloadTaskTvShowSeasonsCommandValidator()
    {
        RuleFor(x => x.DownloadMedias).NotNull();
        RuleFor(x => x.DownloadMedias).NotEmpty();
    }
}

public class GenerateDownloadTaskTvShowSeasonsCommandHandler : IRequestHandler<GenerateDownloadTaskTvShowSeasonsCommand, Result>
{
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;
    private readonly IMediator _mediator;

    public GenerateDownloadTaskTvShowSeasonsCommandHandler(ILog log, IPlexRipperDbContext dbContext, IMediator mediator)
    {
        _log = log;
        _dbContext = dbContext;
        _mediator = mediator;
    }

    public async Task<Result> Handle(GenerateDownloadTaskTvShowSeasonsCommand command, CancellationToken cancellationToken)
    {
        var groupedList = command.DownloadMedias.MergeAndGroupList();
        var plexSeasonList = groupedList.FindAll(x => x.Type == PlexMediaType.Season);
        if (!plexSeasonList.Any())
            return ResultExtensions.IsEmpty(nameof(plexSeasonList)).LogWarning();

        _log.Debug("Creating {PlexTvShowIdsCount} season download tasks", plexSeasonList
            .SelectMany(x => x.MediaIds)
            .ToList()
            .Count);

        var episodesIds = new List<DownloadMediaDTO>();

        foreach (var downloadMediaDto in plexSeasonList)
        {
            var plexLibrary = await _dbContext.PlexLibraries
                .Include(x => x.PlexServer)
                .Include(x => x.DefaultDestination)
                .GetAsync(downloadMediaDto.PlexLibraryId, cancellationToken);
            var plexServer = plexLibrary.PlexServer;

            var plexTvShowSeasons = await _dbContext.PlexTvShowSeason.IncludeAll()
                .Where(x => downloadMediaDto.MediaIds.Contains(x.Id))
                .ToListAsync(cancellationToken);

            foreach (var season in plexTvShowSeasons)
            {
                // Check if the tvShowDownloadTask has already been created
                var downloadTaskTvShow = await _dbContext.GetDownloadTaskTvShowByMediaKeyQuery(season.PlexServerId, season.TvShow.Key, cancellationToken);
                if (downloadTaskTvShow is null)
                {
                    // Insert the tvShowDownloadTask into the database
                    downloadTaskTvShow = season.TvShow.MapToDownloadTask();
                    _dbContext.DownloadTaskTvShow.Add(downloadTaskTvShow);
                    await _dbContext.SaveChangesAsync(cancellationToken);
                }

                // Check if the SeasonDownloadTask has already been created
                var downloadTaskTvShowSeason = downloadTaskTvShow.Children
                    .Find(x => x.PlexServerId == plexServer.Id && x.Key == season.Key);
                if (downloadTaskTvShowSeason is null)
                {
                    var seasonDownloadTask = season.MapToDownloadTask();
                    seasonDownloadTask.ParentId = downloadTaskTvShow.Id;
                    _dbContext.DownloadTaskTvShowSeason.Add(seasonDownloadTask);
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    downloadTaskTvShow.Children.Add(seasonDownloadTask);
                }
            }
        }

        // Create episodes downloadTasks
        await _mediator.Send(new GenerateDownloadTaskTvShowEpisodesCommand(episodesIds), cancellationToken);

        return Result.Ok();
    }
}