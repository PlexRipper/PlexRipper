using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Reaparr.BackgroundJobs.Contracts;
using Reaparr.Data.Contracts;

namespace Reaparr.BackgroundJobs;

public class CheckQueuedPlexLibraryToSyncCommandValidator : AbstractValidator<CheckQueuedPlexLibraryToSyncCommand>
{
    public CheckQueuedPlexLibraryToSyncCommandValidator()
    {
        RuleFor(x => x).NotNull();
    }
}

public class CheckQueuedPlexLibraryToSyncCommandHandler : ICommandHandler<CheckQueuedPlexLibraryToSyncCommand, Result>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly IScheduler _scheduler;
    private readonly ICommandExecutor _commandExecutor;

    public CheckQueuedPlexLibraryToSyncCommandHandler(
        ILogger log,
        IReaparrDbContext dbContext,
        IScheduler scheduler,
        ICommandExecutor commandExecutor
    )
    {
        _log = log.ForContext<CheckQueuedPlexLibraryToSyncCommandHandler>();
        _dbContext = dbContext;
        _scheduler = scheduler;
        _commandExecutor = commandExecutor;
    }

    public async Task<Result> ExecuteAsync(
        CheckQueuedPlexLibraryToSyncCommand command,
        CancellationToken cancellationToken
    )
    {
        var queuedLibraries = await _dbContext
            .LibrarySyncJobQueues.Where(x => x.Status == LibrarySyncQueueStatus.Queued)
            .OrderBy(x => x.Priority)
            .ToListAsync(cancellationToken: cancellationToken);

        _log.Debug("Found {Count} queued libraries to sync the media for", queuedLibraries.Count);

        if (!queuedLibraries.Any())
        {
            _log.Here().Debug("No queued libraries found to sync the media for");
            return Result.Ok();
        }

        var nextLibrary = queuedLibraries.First();

        return await ScheduleLibrarySyncJob(nextLibrary.PlexServerId, nextLibrary.PlexLibraryId);
    }

    private async Task<Result> ScheduleLibrarySyncJob(int serverId, int libraryId)
    {
        var jobKey = LibrarySyncJob.GetJobKey(serverId, libraryId);

        // Check if a job already exists
        if (await _scheduler.CheckExists(jobKey))
        {
            _log.Here()
                .Warning(
                    "Library sync job already exists for server {ServerId}, library {LibraryId}",
                    serverId,
                    libraryId
                );
            return Result.Ok();
        }

        var jobDataMap = new JobDataMap
        {
            [LibrarySyncJob.ServerIdParameter] = serverId,
            [LibrarySyncJob.LibraryIdParameter] = libraryId,
        };

        var job = JobBuilder.Create<LibrarySyncJob>().WithIdentity(jobKey).SetJobData(jobDataMap).Build();

        var trigger = TriggerBuilder.Create().WithIdentity($"{jobKey.Name}_trigger", jobKey.Group).StartNow().Build();

        await _scheduler.ScheduleJob(job, trigger);

        var serverName = await _dbContext.GetPlexServerNameById(serverId);
        var libraryName = await _dbContext.GetPlexLibraryNameById(libraryId);

        _log.Here()
            .Debug(
                "Scheduled library sync job for server {ServerName} with id {ServerId} and library {LibraryName} with {LibraryId}",
                serverName,
                serverId,
                libraryName,
                libraryId
            );

        return Result.Ok();
    }
}
