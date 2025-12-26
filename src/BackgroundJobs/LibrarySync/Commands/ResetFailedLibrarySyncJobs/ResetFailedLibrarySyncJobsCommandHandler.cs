using FastEndpoints;
using FluentValidation;
using Reaparr.BackgroundJobs.Contracts;
using Reaparr.Data.Contracts;

namespace Reaparr.BackgroundJobs;

public class ResetFailedLibrarySyncJobsCommandValidator : AbstractValidator<ResetFailedLibrarySyncJobsCommand>
{
    public ResetFailedLibrarySyncJobsCommandValidator()
    {
        RuleFor(x => x.PlexServerId).GreaterThan(0);
    }
}

public class ResetFailedLibrarySyncJobsCommandHandler : ICommandHandler<ResetFailedLibrarySyncJobsCommand, Result>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly ICommandExecutor _commandExecutor;

    public ResetFailedLibrarySyncJobsCommandHandler(
        ILogger log,
        IReaparrDbContext dbContext,
        ICommandExecutor commandExecutor
    )
    {
        _log = log.ForContext<ResetFailedLibrarySyncJobsCommandHandler>();
        _dbContext = dbContext;
        _commandExecutor = commandExecutor;
    }

    public async Task<Result> ExecuteAsync(
        ResetFailedLibrarySyncJobsCommand command,
        CancellationToken cancellationToken
    )
    {
        var plexServerName = await _dbContext.GetPlexServerNameById(command.PlexServerId, cancellationToken);

        // Reset failed library sync jobs for this server to queue
        var resetCount = await _dbContext
            .LibrarySyncJobQueues.Where(x =>
                x.PlexServerId == command.PlexServerId && x.Status == LibrarySyncJobStatus.Failed
            )
            .ResetJobsToQueuedAsync(cancellationToken);

        if (resetCount > 0)
        {
            _log.Here()
                .Information(
                    "Reset {Count} failed library sync jobs to queued for server {PlexServerName}",
                    resetCount,
                    plexServerName
                );
        }

        await _commandExecutor.Send(new CheckQueuedPlexLibraryToSyncCommand(), cancellationToken);

        return Result.Ok();
    }
}


