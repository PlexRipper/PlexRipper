using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Reaparr.Application.Contracts;
using Reaparr.Application.Contracts.Validators;
using Reaparr.Data.Contracts;

namespace Reaparr.Application;

public record GenerateDownloadTaskTvShowSeasonsCommand(CreateDownloadTasksRequest Request) : ICommand<Result>;

public class GenerateDownloadTaskTvShowSeasonsCommandValidator
    : AbstractValidator<GenerateDownloadTaskTvShowSeasonsCommand>
{
    public GenerateDownloadTaskTvShowSeasonsCommandValidator()
    {
        RuleFor(x => x.Request.DownloadMedias).NotNull();
        RuleFor(x => x.Request.DownloadMedias).NotEmpty();
        RuleForEach(x => x.Request.DownloadMedias).SetValidator(new DownloadMediaDTOValidator());
    }
}

public class GenerateDownloadTaskTvShowSeasonsCommandHandler
    : ICommandHandler<GenerateDownloadTaskTvShowSeasonsCommand, Result>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly ICommandExecutor _command;

    public GenerateDownloadTaskTvShowSeasonsCommandHandler(
        ILogger log,
        IReaparrDbContext dbContext,
        ICommandExecutor command
    )
    {
        _log = log.ForContext<GenerateDownloadTaskTvShowSeasonsCommandHandler>();
        _dbContext = dbContext;
        _command = command;
    }

    public async Task<Result> ExecuteAsync(
        GenerateDownloadTaskTvShowSeasonsCommand command,
        CancellationToken cancellationToken
    )
    {
        var groupedList = command.Request.DownloadMedias.MergeAndGroupList();
        var plexSeasonList = groupedList.FindAll(x => x.Type == PlexMediaType.Season);
        if (!plexSeasonList.Any())
            return ResultExtensions.IsEmpty(nameof(plexSeasonList)).LogWarning();

        _log.Here()
            .Debug(
                "Creating {PlexTvShowIdsCount} season download tasks",
                plexSeasonList.SelectMany(x => x.MediaIds).ToList().Count
            );

        var episodesIds = new List<DownloadMediaDTO>();
        var seasonsToInsert = new List<DownloadTaskTvShowSeason>();

        foreach (var downloadMediaDto in plexSeasonList)
        {
            var plexLibrary = await _dbContext
                .PlexLibraries.Include(x => x.PlexServer)
                .Include(x => x.DefaultDestination)
                .GetAsync(downloadMediaDto.PlexLibraryId, cancellationToken);
            if (plexLibrary is null)
            {
                ResultExtensions.EntityNotFound(nameof(PlexLibrary), downloadMediaDto.PlexLibraryId).LogError();
                continue;
            }

            var plexServer = plexLibrary.PlexServer!;

            var plexTvShowSeasons = await _dbContext
                .PlexTvShowSeason.IncludeAll()
                .Where(x => downloadMediaDto.MediaIds.Contains(x.Id))
                .ToListAsync(cancellationToken);

            foreach (var season in plexTvShowSeasons)
            {
                // Check if the tvShowDownloadTask has already been created
                var downloadTaskTvShow = await _dbContext.GetDownloadTaskTvShowByMediaKeyQuery(
                    season.PlexServerId,
                    season.TvShow!.Key,
                    cancellationToken
                );
                if (downloadTaskTvShow is null)
                {
                    // Insert the tvShowDownloadTask into the database
                    downloadTaskTvShow = season.TvShow.MapToDownloadTask();
                    _dbContext.DownloadTaskTvShow.Add(downloadTaskTvShow);
                    await _dbContext.SaveChangesAsync(cancellationToken);
                }

                // Check if the SeasonDownloadTask has already been created
                var downloadTaskTvShowSeason = downloadTaskTvShow.Children.FirstOrDefault(x =>
                    x.PlexServerId == plexServer.Id && x.PlexId == season.Key
                );
                if (downloadTaskTvShowSeason is null)
                {
                    var seasonDownloadTask = season.MapToDownloadTask();
                    seasonDownloadTask.ParentId = downloadTaskTvShow.Id;
                    seasonsToInsert.Add(seasonDownloadTask);
                }

                episodesIds.Add(
                    new DownloadMediaDTO
                    {
                        MediaIds = season.Episodes.Select(x => x.Id).ToList(),
                        PlexLibraryId = season.PlexLibraryId,
                        PlexServerId = season.PlexServerId,
                        Type = PlexMediaType.Episode,
                        Qualities = [],
                    }
                );
            }
        }

        _dbContext.DownloadTaskTvShowSeason.AddRange(seasonsToInsert);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Create episodes downloadTasks
        await _command.Send(
            new GenerateDownloadTaskTvShowEpisodesCommand(
                new CreateDownloadTasksRequest(episodesIds, command.Request.DestinationFolderPathId)
            ),
            cancellationToken
        );

        return Result.Ok();
    }
}
