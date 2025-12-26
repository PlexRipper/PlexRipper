using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Reaparr.Application.Contracts;
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
    private readonly ISignalRService _signalRService;

    public CleanupLibrarySyncJobQueueCommandHandler(
        ILogger log,
        IReaparrDbContext dbContext,
        ISignalRService signalRService
    )
    {
        _log = log.ForContext<CleanupLibrarySyncJobQueueCommandHandler>();
        _dbContext = dbContext;
        _signalRService = signalRService;
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
            .ResetJobsToQueuedAsync(cancellationToken);

        await _signalRService.SendRefreshNotificationAsync([RefreshDataType.PlexLibrarySyncStatus], cancellationToken);

        _log.Here().Debug("Cleaned up library sync job queue.");

        return Result.Ok();
    }
}
