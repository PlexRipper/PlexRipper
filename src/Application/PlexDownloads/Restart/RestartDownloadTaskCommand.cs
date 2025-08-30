using Data.Contracts;
using FastEndpoints;
using FluentValidation;

namespace PlexRipper.Application;

/// <summary>
/// Restart the <see cref="DownloadTaskGeneric"/> by deleting the PlexDownloadClient and starting a new one.
/// </summary>
/// <param name="DownloadTaskGuid">The id of the <see cref="DownloadTaskGeneric"/> to restart.</param>
/// <returns>Is successful.</returns>
public record RestartDownloadTaskCommand(Guid DownloadTaskGuid) : ICommand<Result>;

public class RestartDownloadTaskCommandValidator : AbstractValidator<RestartDownloadTaskCommand>
{
    public RestartDownloadTaskCommandValidator()
    {
        RuleFor(x => x.DownloadTaskGuid).NotEmpty();
    }
}

public class RestartDownloadTaskCommandHandler : ICommandHandler<RestartDownloadTaskCommand, Result>
{
    private readonly IPlexRipperDbContext _dbContext;
    private readonly ICommandExecutor _commandExecutor;
    private readonly IEventPublisher _eventPublisher;

    public RestartDownloadTaskCommandHandler(
        IPlexRipperDbContext dbContext,
        ICommandExecutor commandExecutor,
        IEventPublisher eventPublisher
    )
    {
        _dbContext = dbContext;
        _commandExecutor = commandExecutor;
        _eventPublisher = eventPublisher;
    }

    public async Task<Result> ExecuteAsync(RestartDownloadTaskCommand command, CancellationToken cancellationToken)
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

            var stopResult = await _commandExecutor.Send(new StopDownloadTaskCommand(childKey.Id), cancellationToken);

            if (stopResult.IsFailed)
                return stopResult.LogError();

            await _dbContext.SetDownloadStatus(childKey, DownloadStatus.Queued);

            await _commandExecutor.Send(new DownloadTaskUpdatedCommand(childKey), cancellationToken);
        }

        await _eventPublisher.PublishAsync(
            new CheckDownloadQueueEvent(downloadTaskKey.PlexServerId),
            cancellationToken
        );

        return Result.Ok();
    }
}
