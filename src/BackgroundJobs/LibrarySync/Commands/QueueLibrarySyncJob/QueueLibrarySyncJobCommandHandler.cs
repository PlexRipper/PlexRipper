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
        // TODO Remove extra where clause once we support other library types
        var libraries = await _dbContext
            .PlexLibraries.Where(x => command.PlexLibraryIds.Contains(x.Id))
            .Where(x => x.Type == PlexMediaType.Movie || x.Type == PlexMediaType.TvShow)
            .Select(x => new
            {
                x.Id,
                x.PlexServerId,
                x.Type,
            })
            .ToListAsync(cancellationToken);

        if (!libraries.Any())
        {
            _log.Here().Warning("No libraries found for the provided library IDs. Nothing to queue.");
            return Result.Ok();
        }

        var existingQueues = await _dbContext
            .LibrarySyncJobQueues.Where(x => command.PlexLibraryIds.Contains(x.PlexLibraryId))
            .ToListAsync(cancellationToken: cancellationToken);

        // Get IDs of items to reset (completed/failed)
        var itemsToReset = existingQueues
            .Where(x => x.Status is LibrarySyncJobStatus.Failed or LibrarySyncJobStatus.Completed)
            .Select(x => x.PlexLibraryId)
            .ToList();

        // Reset completed/failed items
        if (itemsToReset.Any())
        {
            await _dbContext.LibrarySyncJobQueues.ResetLibrarySyncJobQueue(itemsToReset, token: cancellationToken);
        }

        // Get ALL existing library IDs (including those we just reset, which are now Queued)
        var existingLibraryIds = existingQueues.Select(x => x.PlexLibraryId).ToHashSet();

        // Only log warning if there are items that are queued/processing (not just completed/failed)
        var queuedOrProcessingIds = existingQueues
            .Where(x => x.Status is LibrarySyncJobStatus.Queued or LibrarySyncJobStatus.Processing)
            .Select(x => x.PlexLibraryId)
            .ToHashSet();

        if (queuedOrProcessingIds.Any())
        {
            _log.Here()
                .Warning(
                    "{Count} libraries are already queued or processing. They will be skipped.",
                    queuedOrProcessingIds.Count
                );
        }

        // Add new items (excluding ALL existing ones, including those we just reset)
        var itemsToAdd = libraries
            .Where(x => !existingLibraryIds.Contains(x.Id))
            .Select(x => new LibrarySyncJobQueue
            {
                PlexLibraryId = x.Id,
                PlexServerId = x.PlexServerId,
                Priority = GetPriority(x.Type),
                Status = LibrarySyncJobStatus.Queued,
                CreatedAt = DateTime.UtcNow,
            })
            .ToList();

        if (itemsToAdd.Any())
        {
            await _dbContext.LibrarySyncJobQueues.AddRangeAsync(itemsToAdd, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _log.Here()
                .Debug(
                    "Queued {Count} libraries for sync. Reset {ResetCount} completed/failed items. Skipped {SkippedCount} already queued/processing.",
                    itemsToAdd.Count,
                    itemsToReset.Count,
                    queuedOrProcessingIds.Count
                );
        }

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
