using Application.Contracts;
using Data.Contracts;
using DownloadManager.Contracts;
using FluentValidation;
using Logging.Interface;

namespace PlexRipper.Application;

/// <summary>
/// Starts a queued task immediately.
/// </summary>
/// <param name="DownloadTaskId">The ids of the <see cref="DownloadTask"/> to start.</param>
/// <returns>Is successful.</returns>
public record ResumeDownloadTaskCommand(int DownloadTaskId) : IRequest<Result>;

public class ResumeDownloadTaskCommandValidator : AbstractValidator<ResumeDownloadTaskCommand>
{
    public ResumeDownloadTaskCommandValidator()
    {
        RuleFor(x => x.DownloadTaskId).GreaterThan(0);
    }
}

public class ResumeDownloadTaskCommandHandler : IRequestHandler<ResumeDownloadTaskCommand, Result>
{
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;
    private readonly IMediator _mediator;
    private readonly IDownloadTaskScheduler _downloadTaskScheduler;

    public ResumeDownloadTaskCommandHandler(ILog log, IPlexRipperDbContext dbContext, IMediator mediator, IDownloadTaskScheduler downloadTaskScheduler)
    {
        _log = log;
        _dbContext = dbContext;
        _mediator = mediator;
        _downloadTaskScheduler = downloadTaskScheduler;
    }

    public async Task<Result> Handle(ResumeDownloadTaskCommand command, CancellationToken cancellationToken)
    {
        var downloadTask = await _dbContext.DownloadTasks.GetAsync(command.DownloadTaskId, cancellationToken);
        if (downloadTask is null)
            return ResultExtensions.EntityNotFound(nameof(DownloadTask), command.DownloadTaskId).LogError();

        // Don't allow more than 2 downloads at a time from the same server
        if (await _downloadTaskScheduler.IsServerDownloading(downloadTask.PlexServerId))
        {
            var plexServer = await _dbContext.PlexServers.GetAsync(downloadTask.PlexServerId, cancellationToken);
            if (plexServer is null)
                return ResultExtensions.EntityNotFound(nameof(PlexServer), downloadTask.PlexServerId).LogError();

            return Result.Fail($"PlexServer {plexServer.Name} already has a DownloadTask downloading so another one cannot be started").LogWarning();
        }

        // TODO This here should pause other download tasks from the same server and start this one

        await _mediator.Publish(new CheckDownloadQueue(downloadTask.PlexServerId), cancellationToken);

        return Result.Ok();
    }
}