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
        var serverName = await _dbContext.GetPlexServerNameById(command.PlexServerId, ct);
        var libraryName = await _dbContext.GetPlexLibraryNameById(command.LibraryId, ct);

        _log.Here()
            .Information(
                "Forcing library sync for server {ServerName} with id {PlexServerId}, library {LibraryName} with id {LibraryId}",
                serverName,
                command.PlexServerId,
                libraryName,
                command.LibraryId
            );

        // Find currently executing library sync jobs for this server
        var executingJobs = await _scheduler.GetCurrentlyExecutingJobs(ct);
        var serverExecutingJobs = executingJobs
            .Where(x =>
                x.JobInstance is LibrarySyncJob
                && x.JobDetail.JobDataMap.ContainsKey(LibrarySyncJob.ServerIdParameter)
                && x.JobDetail.JobDataMap.GetInt(LibrarySyncJob.ServerIdParameter) == command.PlexServerId
            )
            .ToList();

        // Interrupt any currently executing job for this server
        foreach (var executingJob in serverExecutingJobs)
        {
            if (!executingJob.JobDetail.JobDataMap.ContainsKey(LibrarySyncJob.LibraryIdParameter))
                continue;

            var executingLibraryId = executingJob.JobDetail.JobDataMap.GetInt(LibrarySyncJob.LibraryIdParameter);
            var executingLibraryName = await _dbContext.GetPlexLibraryNameById(executingLibraryId, ct);

            _log.Here()
                .Information(
                    "Interrupting currently executing library sync for server {ServerName} with id {PlexServerId}, library {ExecutingLibraryName} with id {ExecutingLibraryId}",
                    serverName,
                    command.PlexServerId,
                    executingLibraryName,
                    executingLibraryId
                );

            await _scheduler.Interrupt(executingJob.JobDetail.Key, ct);
        }

        // Get the target job key
        var targetJobKey = LibrarySyncJob.GetJobKey(command.PlexServerId, command.LibraryId);

        // Delete existing job/trigger if it exists
        if (await _scheduler.CheckExists(targetJobKey, ct))
        {
            await _scheduler.DeleteJob(targetJobKey, ct);
        }

        // Get all triggers for LibrarySync group to determine max priority
        var jobGroupMatcher = GroupMatcher<JobKey>.GroupEquals("LibrarySync");
        var allJobKeys = await _scheduler.GetJobKeys(jobGroupMatcher, ct);
        var allTriggers = new List<ITrigger>();

        foreach (var jobKey in allJobKeys)
        {
            var triggers = await _scheduler.GetTriggersOfJob(jobKey, ct);
            allTriggers.AddRange(triggers);
        }

        var maxPriority = allTriggers.Any() ? allTriggers.Max(t => t.Priority) : 0;

        // Schedule the target library with highest priority
        var jobDataMap = new JobDataMap
        {
            [LibrarySyncJob.ServerIdParameter] = command.PlexServerId,
            [LibrarySyncJob.LibraryIdParameter] = command.LibraryId,
        };

        var job = JobBuilder
            .Create<LibrarySyncJob>()
            .WithIdentity(targetJobKey)
            .SetJobData(jobDataMap)
            .Build();

        var trigger = TriggerBuilder
            .Create()
            .WithIdentity($"{targetJobKey.Name}_trigger", targetJobKey.Group)
            .WithPriority(maxPriority + 1000) // Much higher priority to ensure it runs next
            .StartNow()
            .Build();

        await _scheduler.ScheduleJob(job, trigger, ct);

        _log.Here()
            .Information(
                "Rescheduled library sync with highest priority for server {ServerName} with id {PlexServerId}, library {LibraryName} with id {LibraryId}",
                serverName,
                command.PlexServerId,
                libraryName,
                command.LibraryId
            );

        return Result.Ok();
    }
}

