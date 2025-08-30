using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Reaparr.Application.Contracts;
using Reaparr.Application.Contracts.Validators;
using Reaparr.Data.Contracts;
using ILog = Reaparr.Logging.ILog;

namespace Reaparr.Application;

public record GenerateDownloadTaskTvShowsCommand : ICommand<Result>
{
    public GenerateDownloadTaskTvShowsCommand(CreateDownloadTasksRequest request)
    {
        Request = request;
    }

    public GenerateDownloadTaskTvShowsCommand(List<DownloadMediaDTO> downloadMediaDtos)
    {
        Request = new CreateDownloadTasksRequest(downloadMediaDtos);
    }

    public CreateDownloadTasksRequest Request { get; }
}

public class GenerateDownloadTaskTvShowsCommandValidator : AbstractValidator<GenerateDownloadTaskTvShowsCommand>
{
    public GenerateDownloadTaskTvShowsCommandValidator()
    {
        RuleFor(x => x.Request.DownloadMedias).NotNull();
        RuleFor(x => x.Request.DownloadMedias).NotEmpty();
        RuleForEach(x => x.Request.DownloadMedias).SetValidator(new DownloadMediaDTOValidator());
    }
}

public class GenerateDownloadTaskTvShowsCommandHandler : ICommandHandler<GenerateDownloadTaskTvShowsCommand, Result>
{
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;
    private readonly ICommandExecutor _commandExecutor;

    public GenerateDownloadTaskTvShowsCommandHandler(
        ILog log,
        IPlexRipperDbContext dbContext,
        ICommandExecutor commandExecutor
    )
    {
        _log = log;
        _dbContext = dbContext;
        _commandExecutor = commandExecutor;
    }

    public async Task<Result> ExecuteAsync(
        GenerateDownloadTaskTvShowsCommand command,
        CancellationToken cancellationToken
    )
    {
        var groupedList = command.Request.DownloadMedias.MergeAndGroupList();
        var plexTvShowList = groupedList.FindAll(x => x.Type == PlexMediaType.TvShow);
        if (!plexTvShowList.Any())
            return ResultExtensions.IsEmpty(nameof(plexTvShowList)).LogWarning();

        _log.Debug(
            "Creating {PlexTvShowIdsCount} TvShow download tasks",
            plexTvShowList.SelectMany(x => x.MediaIds).ToList().Count
        );

        foreach (var downloadMediaDto in plexTvShowList)
        {
            var plexTvShows = await _dbContext
                .PlexTvShows.Include(x => x.Seasons)
                .Where(x => downloadMediaDto.MediaIds.Contains(x.Id))
                .ToListAsync(cancellationToken);

            var seasonsIds = new List<DownloadMediaDTO>();
            var tvShowsToInsert = new List<DownloadTaskTvShow>();

            foreach (var tvShow in plexTvShows)
            {
                // Check if the tvShowDownloadTask has already been created
                var downloadTaskTvShow = await _dbContext.GetDownloadTaskTvShowByMediaKeyQuery(
                    tvShow.PlexServerId,
                    tvShow.Key,
                    cancellationToken
                );

                if (downloadTaskTvShow is null)
                {
                    downloadTaskTvShow = tvShow.MapToDownloadTask();

                    tvShowsToInsert.Add(downloadTaskTvShow);
                    seasonsIds.Add(
                        new DownloadMediaDTO
                        {
                            MediaIds = tvShow.Seasons.Select(x => x.Id).ToList(),
                            PlexLibraryId = tvShow.PlexLibraryId,
                            PlexServerId = tvShow.PlexServerId,
                            Type = PlexMediaType.Season,
                            Qualities = [],
                        }
                    );
                }
            }

            // Insert the tvShowDownloadTask into the database
            _dbContext.DownloadTaskTvShow.AddRange(tvShowsToInsert);
            await _dbContext.SaveChangesAsync(cancellationToken);

            // Create seasons downloadTasks
            await _commandExecutor.Send(
                new GenerateDownloadTaskTvShowSeasonsCommand(
                    new CreateDownloadTasksRequest(seasonsIds, command.Request.DestinationFolderPathId)
                ),
                cancellationToken
            );
        }

        return Result.Ok();
    }
}
