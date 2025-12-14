using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Reaparr.BackgroundJobs.Contracts;
using Reaparr.Data.Contracts;

namespace Reaparr.BackgroundJobs;

public class CleanupLibrarySyncJobQueueCommandValidator : AbstractValidator<CleanupLibrarySyncJobQueueCommand>
{
    public CleanupLibrarySyncJobQueueCommandValidator()
    {
        RuleFor(x => x).NotNull();
    }
}

public class CleanupLibrarySyncJobQueueCommandHandler : ICommandHandler<CleanupLibrarySyncJobQueueCommand, Result>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly ICommandExecutor _commandExecutor;

    public CleanupLibrarySyncJobQueueCommandHandler(
        ILogger log,
        IReaparrDbContext dbContext,
        ICommandExecutor commandExecutor
    )
    {
        _log = log.ForContext<CleanupLibrarySyncJobQueueCommandHandler>();
        _dbContext = dbContext;
        _commandExecutor = commandExecutor;
    }

    public async Task<Result> ExecuteAsync(
        CleanupLibrarySyncJobQueueCommand command,
        CancellationToken cancellationToken
    )
    {
        // Cleanup completed queue items
        await _dbContext
            .LibrarySyncJobQueues.Where(x => x.Status == LibrarySyncJobStatus.Completed)
            .ExecuteDeleteAsync(cancellationToken: cancellationToken);

        // Re-queue failed or processing items
        await _dbContext
            .LibrarySyncJobQueues.Where(x =>
                x.Status == LibrarySyncJobStatus.Failed || x.Status == LibrarySyncJobStatus.Processing
            )
            .ExecuteUpdateAsync(
                x =>
                    x.SetProperty(y => y.Status, LibrarySyncJobStatus.Queued)
                        .SetProperty(y => y.StartedAt, (DateTime?)null)
                        .SetProperty(y => y.ErrorMessage, (string?)null),
                cancellationToken: cancellationToken
            );

        _log.Here().Debug("Cleaned up library sync job queue.");

        return Result.Ok();
    }
}
