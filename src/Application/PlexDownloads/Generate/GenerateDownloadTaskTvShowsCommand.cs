using Application.Contracts;
using Application.Contracts.Validators;
using Data.Contracts;
using FluentValidation;
using Logging.Interface;

namespace PlexRipper.Application;

public record GenerateDownloadTaskTvShowsCommand(List<DownloadMediaDTO> DownloadMedias) : IRequest<Result>;

public class GenerateDownloadTaskTvShowsCommandValidator : AbstractValidator<GenerateDownloadTaskTvShowsCommand>
{
    public GenerateDownloadTaskTvShowsCommandValidator()
    {
        RuleFor(x => x.DownloadMedias).NotNull();
        RuleFor(x => x.DownloadMedias).NotEmpty();
        RuleForEach(x => x.DownloadMedias).SetValidator(new DownloadMediaDTOValidator());
    }
}

public class GenerateDownloadTaskTvShowsCommandHandler : IRequestHandler<GenerateDownloadTaskTvShowsCommand, Result>
{
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;
    private readonly IMediator _mediator;

    public GenerateDownloadTaskTvShowsCommandHandler(ILog log, IPlexRipperDbContext dbContext, IMediator mediator)
    {
        _log = log;
        _dbContext = dbContext;
        _mediator = mediator;
    }

    public async Task<Result> Handle(GenerateDownloadTaskTvShowsCommand command, CancellationToken cancellationToken)
    {
        var groupedList = command.DownloadMedias.MergeAndGroupList();
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
                .PlexTvShows.IncludeSeasons()
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
                        }
                    );
                }
            }

            // Insert the tvShowDownloadTask into the database
            _dbContext.DownloadTaskTvShow.AddRange(tvShowsToInsert);
            await _dbContext.SaveChangesAsync(cancellationToken);

            // Create seasons downloadTasks
            await _mediator.Send(new GenerateDownloadTaskTvShowSeasonsCommand(seasonsIds), cancellationToken);
        }

        return Result.Ok();
    }
}
