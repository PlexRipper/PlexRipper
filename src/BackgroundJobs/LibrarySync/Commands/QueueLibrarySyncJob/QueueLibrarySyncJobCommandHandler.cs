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
        var libraries = await _dbContext
            .PlexLibraries.Where(x => command.PlexLibraryIds.Contains(x.Id))
            .Select(x => new
            {
                x.Id,
                x.PlexServerId,
                x.Type,
            })
            .ToListAsync(cancellationToken);

        var existingQueues = await _dbContext
            .LibrarySyncJobQueues.Where(x => command.PlexLibraryIds.Contains(x.PlexLibraryId))
            .ToListAsync(cancellationToken: cancellationToken);

        if (existingQueues.Any())
        {
            _log.Here()
                .Warning(
                    "Some libraries are already queued for sync. Existing queue items will be updated if they are completed/failed."
                );

            var exitingIds = existingQueues
                .Where(x => x.Status is LibrarySyncJobStatus.Failed or LibrarySyncJobStatus.Completed)
                .Select(x => x.PlexLibraryId)
                .ToList();

            await _dbContext.LibrarySyncJobQueues.ResetLibrarySyncJobQueue(exitingIds, token: cancellationToken);
        }

        var exitingIds2 = existingQueues
            .Where(x => x.Status is LibrarySyncJobStatus.Queued or LibrarySyncJobStatus.Processing)
            .Select(x => x.PlexLibraryId)
            .ToList();

        var itemsToAdd = libraries
            .Where(x => !exitingIds2.Contains(x.Id))
            .Select(x => new LibrarySyncJobQueue
            {
                PlexLibraryId = x.Id,
                PlexServerId = x.PlexServerId,
                Priority = GetPriority(x.Type),
                Status = LibrarySyncJobStatus.Queued,
                CreatedAt = DateTime.UtcNow,
            })
            .ToList();

        await _dbContext.LibrarySyncJobQueues.AddRangeAsync(itemsToAdd, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

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
