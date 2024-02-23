using BackgroundServices.Contracts;
using Data.Contracts;
using DownloadManager.Contracts;
using FluentValidation;

namespace PlexRipper.Application;

public record StartDownloadTaskCommand(int DownloadTaskId) : IRequest<Result>;

public class StartDownloadTaskCommandValidator : AbstractValidator<StartDownloadTaskCommand>
{
    public StartDownloadTaskCommandValidator()
    {
        RuleFor(x => x.DownloadTaskId).GreaterThan(0);
    }
}

public class StartDownloadTaskCommandHandler : IRequestHandler<StartDownloadTaskCommand, Result>
{
    private readonly IPlexRipperDbContext _dbContext;
    private readonly IMediator _mediator;
    private readonly IDownloadTaskScheduler _downloadTaskScheduler;

    public StartDownloadTaskCommandHandler(IPlexRipperDbContext dbContext, IMediator mediator, IDownloadTaskScheduler downloadTaskScheduler)
    {
        _dbContext = dbContext;
        _mediator = mediator;
        _downloadTaskScheduler = downloadTaskScheduler;
    }

    public async Task<Result> Handle(StartDownloadTaskCommand command, CancellationToken cancellationToken)
    {
        var downloadTask = await _dbContext.DownloadTasks.GetAsync(command.DownloadTaskId, cancellationToken);
        if (downloadTask is null)
            return ResultExtensions.EntityNotFound(nameof(DownloadTask), command.DownloadTaskId).LogWarning();

        if (downloadTask.IsDownloadable)
            return await _downloadTaskScheduler.StartDownloadTaskJob(downloadTask.Id, downloadTask.PlexServerId);

        await _mediator.Publish(new CheckDownloadQueue(downloadTask.PlexServerId), cancellationToken);

        return Result.Fail($"Failed to start downloadTask {downloadTask.FullTitle}, it's not directly downloadable.");
    }
}