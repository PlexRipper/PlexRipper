using Microsoft.EntityFrameworkCore;
using Reaparr.Data.Contracts;

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
    private readonly IEventPublisher _eventPublisher;

    public ResumePlexServerDownloadsCommandHandler(
        ILogger log,
        IReaparrDbContext dbContext,
        IEventPublisher eventPublisher
    )
    {
        _log = log.ForContext<ResumePlexServerDownloadsCommandHandler>();
        _dbContext = dbContext;
        _eventPublisher = eventPublisher;
    }

    public async Task<Result> ExecuteAsync(
        ResumePlexServerDownloadsCommand command,
        CancellationToken cancellationToken
    )
    {
        var plexServer = await _dbContext.PlexServers.GetAsync(command.PlexServerId, cancellationToken);
        if (plexServer is null)
            return ResultExtensions.EntityNotFound(nameof(PlexServer), command.PlexServerId).LogError();

        _log.Here().Information("Unpausing PlexServer with id: {PlexServerId}", command.PlexServerId);

        var updateResult = await Result.Try(() =>
            _dbContext
                .PlexServers.Where(x => x.Id == command.PlexServerId)
                .ExecuteUpdateAsync(p => p.SetProperty(x => x.IsDownloadsPausedByUser, false), cancellationToken)
        );
        if (updateResult.IsFailed)
            return updateResult.ToResult().LogError();

        var publishResult = await Result.Try(() =>
            _eventPublisher.PublishAsync(new CheckDownloadQueueEvent(command.PlexServerId), cancellationToken)
        );
        if (publishResult.IsFailed)
            return publishResult.LogError();

        return Result.Ok();
    }
}
