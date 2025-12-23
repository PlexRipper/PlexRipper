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

    public CheckQueuedPlexLibraryToSyncCommandHandler(ILogger log, IReaparrDbContext dbContext, IScheduler scheduler)
    {
        _log = log.ForContext<CheckQueuedPlexLibraryToSyncCommandHandler>();
        _dbContext = dbContext;
        _scheduler = scheduler;
    }

    public async Task<Result> ExecuteAsync(
        CheckQueuedPlexLibraryToSyncCommand command,
        CancellationToken cancellationToken
    )
    {
        // Group queued libraries by server
        var librariesByServer = await _dbContext
            .LibrarySyncJobQueues.Where(x => x.Status == LibrarySyncJobStatus.Queued)
            .GroupBy(x => x.PlexServerId)
            .ToListAsync(cancellationToken: cancellationToken);

        if (!librariesByServer.Any())
        {
            _log.Here().Debug("No queued libraries found to sync the media for");
            return Result.Ok();
        }

        var totalLibraries = librariesByServer.Sum(g => g.Count());
        _log.Here()
            .Debug(
                "Found {Count} queued libraries across {ServerCount} servers to sync",
                totalLibraries,
                librariesByServer.Count
            );

        foreach (var serverGroup in librariesByServer)
        {
            var serverId = serverGroup.Key;
            var isServerOnline = await _dbContext.IsServerOnline(serverId, cancellationToken);

            if (!isServerOnline)
            {
                _log.Here()
                    .Warning(
                        "Skipping {Count} library syncs for server {ServerId} because it is offline",
                        serverGroup.Count(),
                        serverId
                    );

                // Mark all queued libraries for this server as waiting for the server to come online
                await _dbContext
                    .LibrarySyncJobQueues.Where(x =>
                        x.PlexServerId == serverId && x.Status == LibrarySyncJobStatus.Queued
                    )
                    .ExecuteUpdateAsync(x => x.SetProperty(y => y.IsServerOffline, true), cancellationToken);

                continue;
            }

            var nextLibrary = serverGroup.OrderBy(x => x.Priority).First();

            // Clear the offline flag for the item being scheduled
            if (nextLibrary.IsServerOffline)
            {
                await _dbContext
                    .LibrarySyncJobQueues.Where(x =>
                        x.PlexServerId == serverId && x.PlexLibraryId == nextLibrary.PlexLibraryId
                    )
                    .ExecuteUpdateAsync(x => x.SetProperty(y => y.IsServerOffline, false), cancellationToken);
            }

            await ScheduleLibrarySyncJob(serverId, nextLibrary.PlexLibraryId);
        }

        return Result.Ok();
    }

    private async Task ScheduleLibrarySyncJob(int serverId, int libraryId)
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
            return;
        }

        var jobDataMap = new JobDataMap
        {
            [LibrarySyncJob.ServerIdParameter] = serverId,
            [LibrarySyncJob.LibraryIdParameter] = libraryId,
        };

        var job = JobBuilder.Create<LibrarySyncJob>().WithIdentity(jobKey).SetJobData(jobDataMap).Build();

        var trigger = TriggerBuilder.Create().WithIdentity($"{jobKey.Name}_trigger", jobKey.Group).StartNow().Build();

        await _scheduler.ScheduleJob(job, trigger);

        _log.Here()
            .Debug("Scheduled library sync job for server {ServerId} and library {LibraryId}", serverId, libraryId);
    }
}
