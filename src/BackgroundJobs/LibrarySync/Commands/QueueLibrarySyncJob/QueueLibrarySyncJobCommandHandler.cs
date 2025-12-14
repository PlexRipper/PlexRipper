using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Reaparr.BackgroundJobs.Contracts;
using Reaparr.Data.Contracts;

namespace Reaparr.BackgroundJobs;

public class QueueLibrarySyncJobCommandValidator : AbstractValidator<QueueLibrarySyncJobCommand>
{
    public QueueLibrarySyncJobCommandValidator()
    {
        RuleFor(x => x).NotNull();
        RuleFor(x => x.PlexLibraryIds).NotEmpty();
    }
}

public class QueueLibrarySyncJobCommandHandler : ICommandHandler<QueueLibrarySyncJobCommand, Result>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly ICommandExecutor _commandExecutor;

    public QueueLibrarySyncJobCommandHandler(ILogger log, IReaparrDbContext dbContext, ICommandExecutor commandExecutor)
    {
        _log = log.ForContext<QueueLibrarySyncJobCommandHandler>();
        _dbContext = dbContext;
        _commandExecutor = commandExecutor;
    }

    public async Task<Result> ExecuteAsync(QueueLibrarySyncJobCommand command, CancellationToken cancellationToken)
    {
        var queuedLibraries = await _dbContext
            .PlexLibraries.Where(x => command.PlexLibraryIds.Contains(x.Id))
            .Select(x => new
            {
                x.Id,
                x.PlexServerId,
                x.Type,
            })
            .ToListAsync(cancellationToken);

        var queueItems = queuedLibraries
            .Select(x => new LibrarySyncJobQueue
            {
                PlexLibraryId = x.Id,
                PlexServerId = x.PlexServerId,
                Priority = GetPriority(x.Type),
                Status = LibrarySyncQueueStatus.Queued,
                CreatedAt = DateTime.UtcNow,
            })
            .ToList();

        await _dbContext.LibrarySyncJobQueues.AddRangeAsync(queueItems, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _log.Here().Debug("Queued {Count} libraries for sync.", queueItems.Count);

        // Trigger check for queued library syncs
        await _commandExecutor.Send(new CheckQueuedPlexLibraryToSyncCommand(), cancellationToken);

        return Result.Ok();
    }

    private int GetPriority(PlexMediaType type)
    {
        return type switch
        {
            PlexMediaType.Movie => 1,
            PlexMediaType.TvShow => 2,
            _ => 3,
        };
    }
}
