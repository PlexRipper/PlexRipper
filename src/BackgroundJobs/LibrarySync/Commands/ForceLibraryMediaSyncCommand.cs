using FastEndpoints;
using FluentValidation;
using Quartz;
using Quartz.Impl.Matchers;
using Reaparr.Data.Contracts;

namespace Reaparr.BackgroundJobs;

/// <summary>
/// Command to move a library sync to the front of the queue by canceling the current execution and rescheduling with the highest priority.
/// </summary>
public record ForceLibraryMediaSyncCommand(int PlexServerId, int LibraryId) : ICommand<Result>;

public class ForceLibraryMediaSyncCommandValidator : AbstractValidator<ForceLibraryMediaSyncCommand>
{
    public ForceLibraryMediaSyncCommandValidator()
    {
        RuleFor(x => x.PlexServerId).GreaterThan(0);
        RuleFor(x => x.LibraryId).GreaterThan(0);
    }
}

public class ForceLibraryMediaSyncCommandHandler : ICommandHandler<ForceLibraryMediaSyncCommand, Result>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly IScheduler _scheduler;

    public ForceLibraryMediaSyncCommandHandler(ILogger log, IReaparrDbContext dbContext, IScheduler scheduler)
    {
        _log = log.ForContext<ForceLibraryMediaSyncCommandHandler>();
        _dbContext = dbContext;
        _scheduler = scheduler;
    }

    public async Task<Result> ExecuteAsync(ForceLibraryMediaSyncCommand command, CancellationToken ct)
    {
        var plexServerId = command.PlexServerId;
        var libraryId = command.LibraryId;
        var serverName = await _dbContext.GetPlexServerNameById(plexServerId, cancellationToken: ct);
        var libraryName = await _dbContext.GetPlexLibraryNameById(libraryId, cancellationToken: ct);

        _log.Here()
            .Information(
                "Forcing library sync of library {libraryName} with id {LibraryId} in server {serverName} with id {PlexServerId}",
                libraryName,
                libraryId,
                serverName,
                plexServerId
            );

        // Find currently executing library sync jobs for this server
        var executingJobs = await _scheduler.GetCurrentlyExecutingJobs(ct);
        var serverExecutingJobs = executingJobs
            .Where(x =>
                x.JobInstance is LibrarySyncJob
                && x.JobDetail.JobDataMap.ContainsKey(LibrarySyncJob.ServerIdParameter)
                && x.JobDetail.JobDataMap.GetInt(LibrarySyncJob.ServerIdParameter) == plexServerId
            )
            .ToList();

        // Interrupt any currently executing job for this server
        foreach (var executingJob in serverExecutingJobs)
        {
            if (!executingJob.JobDetail.JobDataMap.ContainsKey(LibrarySyncJob.LibraryIdParameter))
                continue;

            var executingLibraryId = executingJob.JobDetail.JobDataMap.GetInt(LibrarySyncJob.LibraryIdParameter);

            _log.Here()
                .Information(
                    "Interrupting currently executing library sync for server {PlexServerId}, library {ExecutingLibraryId}",
                    plexServerId,
                    executingLibraryId
                );

            await _scheduler.Interrupt(executingJob.JobDetail.Key, ct);
        }

        // Get the target job key
        var targetJobKey = LibrarySyncJob.GetJobKey(plexServerId, libraryId);

        // Delete existing job/trigger if it exists
        if (await _scheduler.CheckExists(targetJobKey, ct))
        {
            await _scheduler.DeleteJob(targetJobKey, ct);
        }

        // Schedule the target library with highest priority
        var jobDataMap = new JobDataMap
        {
            [LibrarySyncJob.ServerIdParameter] = plexServerId,
            [LibrarySyncJob.LibraryIdParameter] = libraryId,
        };

        var job = JobBuilder.Create<LibrarySyncJob>().WithIdentity(targetJobKey).SetJobData(jobDataMap).Build();

        var trigger = TriggerBuilder
            .Create()
            .WithIdentity($"{targetJobKey.Name}_trigger", targetJobKey.Group)
            .StartNow()
            .Build();

        await _scheduler.ScheduleJob(job, trigger, ct);

        _log.Here()
            .Information(
                "Rescheduled library sync with highest priority for server {PlexServerId}, library {LibraryId}",
                plexServerId,
                libraryId
            );

        return Result.Ok();
    }
}
