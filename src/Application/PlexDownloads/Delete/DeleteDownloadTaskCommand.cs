using Application.Contracts;
using Data.Contracts;
using FluentValidation;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application;

/// <summary>
/// Stops and deletes (active) PlexDownloadClients and removes <see cref="DownloadTask"/> from the database.
/// </summary>
/// <param name="DownloadTaskIds">The list of <see cref="DownloadTask"/> to delete.</param>
/// <returns><see cref="Result"/> fails on error.</returns>
public record DeleteDownloadTaskCommand(List<Guid> DownloadTaskIds) : IRequest<Result>;

public class DeleteDownloadTaskCommandValidator : AbstractValidator<DeleteDownloadTaskCommand>
{
    public DeleteDownloadTaskCommandValidator()
    {
        RuleFor(x => x.DownloadTaskIds).NotNull();
        RuleFor(x => x.DownloadTaskIds).NotEmpty();
    }
}

public class DeleteDownloadTaskCommandHandler : IRequestHandler<DeleteDownloadTaskCommand, Result>
{
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;
    private readonly IMediator _mediator;
    private readonly IDownloadTaskScheduler _downloadTaskScheduler;

    public DeleteDownloadTaskCommandHandler(ILog log, IPlexRipperDbContext dbContext, IMediator mediator, IDownloadTaskScheduler _downloadTaskScheduler)
    {
        _log = log;
        _dbContext = dbContext;
        _mediator = mediator;
        this._downloadTaskScheduler = _downloadTaskScheduler;
    }

    public async Task<Result> Handle(DeleteDownloadTaskCommand command, CancellationToken cancellationToken)
    {
        foreach (var downloadTaskId in command.DownloadTaskIds)
            if (await _downloadTaskScheduler.IsDownloading(downloadTaskId))
                await _mediator.Send(new StopDownloadTaskCommand(downloadTaskId), cancellationToken);

        // Delete Download tasks
        await _dbContext.DownloadTaskMovie.Where(x => command.DownloadTaskIds.Contains(x.Id)).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.DownloadTaskMovieFile.Where(x => command.DownloadTaskIds.Contains(x.Id)).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.DownloadTaskTvShow.Where(x => command.DownloadTaskIds.Contains(x.Id)).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.DownloadTaskTvShowSeason.Where(x => command.DownloadTaskIds.Contains(x.Id)).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.DownloadTaskTvShowEpisode.Where(x => command.DownloadTaskIds.Contains(x.Id)).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.DownloadTaskTvShowEpisodeFile.Where(x => command.DownloadTaskIds.Contains(x.Id)).ExecuteDeleteAsync(cancellationToken);

        return Result.Ok();
    }
}