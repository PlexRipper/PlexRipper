using Data.Contracts;
using FluentValidation;

namespace PlexRipper.Application;

/// <summary>
/// Restart the <see cref="DownloadTaskGeneric"/> by deleting the PlexDownloadClient and starting a new one.
/// </summary>
/// <param name="DownloadTaskGuid">The id of the <see cref="DownloadTaskGeneric"/> to restart.</param>
/// <returns>Is successful.</returns>
public record RestartDownloadTaskCommand(Guid DownloadTaskGuid) : IRequest<Result>;

public class RestartDownloadTaskCommandValidator : AbstractValidator<RestartDownloadTaskCommand>
{
    public RestartDownloadTaskCommandValidator()
    {
        RuleFor(x => x.DownloadTaskGuid).NotEmpty();
    }
}

public class RestartDownloadTaskCommandHandler : IRequestHandler<RestartDownloadTaskCommand, Result>
{
    private readonly IPlexRipperDbContext _dbContext;
    private readonly IMediator _mediator;

    public RestartDownloadTaskCommandHandler(IPlexRipperDbContext dbContext, IMediator mediator)
    {
        _dbContext = dbContext;
        _mediator = mediator;
    }

    public async Task<Result> Handle(RestartDownloadTaskCommand command, CancellationToken cancellationToken)
    {
        var downloadTaskKey = await _dbContext.GetDownloadTaskKeyAsync(command.DownloadTaskGuid, cancellationToken);
        if (downloadTaskKey is null)
            return ResultExtensions.EntityNotFound(nameof(DownloadTaskGeneric), command.DownloadTaskGuid).LogWarning();

        var childKeys = await _dbContext.GetDownloadableChildTaskKeys(downloadTaskKey, cancellationToken);

        foreach (var childKey in childKeys)
        {
            var downloadTask = await _dbContext.GetDownloadTaskAsync(childKey, cancellationToken);
            if (downloadTask is null)
            {
                ResultExtensions.EntityNotFound(nameof(DownloadTaskGeneric), childKey.Id).LogError();
                continue;
            }

            var stopResult = await _mediator.Send(new StopDownloadTaskCommand(childKey.Id), cancellationToken);

            if (stopResult.IsFailed)
                return stopResult;

            // Reset progress of the downloadTask
            await _dbContext.UpdateDownloadProgress(
                childKey,
                new DownloadTaskProgress
                {
                    Percentage = 0,
                    DataReceived = 0,
                    DataTotal = downloadTask.DataTotal,
                    DownloadSpeed = 0,
                },
                cancellationToken
            );

            await _dbContext.SetDownloadStatus(childKey, DownloadStatus.Queued);

            await _mediator.Send(new DownloadTaskUpdatedNotification(childKey), cancellationToken);
        }

        await _mediator.Publish(new CheckDownloadQueueNotification(downloadTaskKey.PlexServerId), cancellationToken);

        return Result.Ok();
    }
}
