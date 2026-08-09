namespace Reaparr.Application;

public record ResumePlexServerDownloadsCommand(int PlexServerId) : ICommand<Result>;

public class ResumePlexServerDownloadsCommandValidator : AbstractValidator<ResumePlexServerDownloadsCommand>
{
    public ResumePlexServerDownloadsCommandValidator()
    {
        RuleFor(x => x.PlexServerId).GreaterThan(0);
    }
}

public class ResumePlexServerDownloadsCommandHandler : ICommandHandler<ResumePlexServerDownloadsCommand, Result>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly IDownloadQueue _downloadQueue;

    public ResumePlexServerDownloadsCommandHandler(
        ILogger log,
        IReaparrDbContext dbContext,
        IDownloadQueue downloadQueue
    )
    {
        _log = log.ForContext<ResumePlexServerDownloadsCommandHandler>();
        _dbContext = dbContext;
        _downloadQueue = downloadQueue;
    }

    public async Task<Result> ExecuteAsync(
        ResumePlexServerDownloadsCommand command,
        CancellationToken cancellationToken
    )
    {
        var plexServer = await _dbContext.PlexServers
            .GetAsync(command.PlexServerId, cancellationToken);
        if (plexServer is null)
            return ResultExtensions.EntityNotFound(nameof(PlexServer), command.PlexServerId).LogError();

        _log.Here().Information("Unpausing PlexServer with id: {PlexServerId}", command.PlexServerId);

        var updateResult = await Result.Try(() =>
            _dbContext.PlexServers
                .Where(x => x.Id == command.PlexServerId)
                .ExecuteUpdateAsync(p => p.SetProperty(x => x.IsDownloadsPausedByUser, false), cancellationToken)
        );
        if (updateResult.IsCancelled)
            return updateResult.ToResult();

        if (updateResult.IsFailed)
            return updateResult.ToResult().LogError();

        // The resume has committed, so its queue wake-up must no longer be tied to the request token.
        // The boot-time all-server queue check provides reconciliation if the process stops before this is queued.
        var queueResult = await Result.Try(() =>
            _downloadQueue.CheckDownloadQueue([command.PlexServerId], CancellationToken.None)
        );
        if (queueResult.IsCancelled)
            return queueResult.LogWarning();

        if (queueResult.IsFailed)
            return queueResult.LogError();

        if (cancellationToken.IsCancellationRequested)
            return ResultExtensions.TaskIsCancelled(nameof(ResumePlexServerDownloadsCommand));

        return Result.Ok();
    }
}