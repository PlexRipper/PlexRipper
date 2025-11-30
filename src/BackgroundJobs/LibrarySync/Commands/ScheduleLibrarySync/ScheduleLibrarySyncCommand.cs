using System.Diagnostics.CodeAnalysis;
using FastEndpoints;
using FluentValidation;
using Quartz;
using Reaparr.Data.Contracts;

namespace Reaparr.BackgroundJobs;

/// <summary>
/// Schedules a library sync job, optionally with remaining library IDs to chain to.
/// </summary>
public record ScheduleLibrarySyncCommand : ICommand<Result>
{
    [SetsRequiredMembers]
    public ScheduleLibrarySyncCommand(int plexServerId, int plexLibraryId)
    {
        PlexServerId = plexServerId;
        PlexLibraryId = plexLibraryId;
    }

    public required int PlexServerId { get; init; }
    public required int PlexLibraryId { get; init; }
}

public class ScheduleLibrarySyncCommandValidator : AbstractValidator<ScheduleLibrarySyncCommand>
{
    public ScheduleLibrarySyncCommandValidator()
    {
        RuleFor(x => x).NotNull();
        RuleFor(x => x.PlexServerId).GreaterThan(0);
        RuleFor(x => x.PlexLibraryId).GreaterThan(0);
    }
}

public class ScheduleLibrarySyncCommandHandler : ICommandHandler<ScheduleLibrarySyncCommand, Result>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly IScheduler _scheduler;

    public ScheduleLibrarySyncCommandHandler(ILogger log, IReaparrDbContext dbContext, IScheduler scheduler)
    {
        _log = log.ForContext<ScheduleLibrarySyncCommandHandler>();
        _dbContext = dbContext;
        _scheduler = scheduler;
    }

    public async Task<Result> ExecuteAsync(ScheduleLibrarySyncCommand command, CancellationToken cancellationToken)
    {
        var serverId = command.PlexServerId;
        var libraryId = command.PlexLibraryId;
        var jobKey = LibrarySyncJob.GetJobKey(serverId, libraryId);

        // Check if a job already exists
        if (await _scheduler.CheckExists(jobKey, cancellationToken))
        {
            _log.Here()
                .Debug(
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

        await _scheduler.ScheduleJob(job, trigger, cancellationToken);

        var serverName = await _dbContext.GetPlexServerNameById(serverId, cancellationToken);
        var libraryName = await _dbContext.GetPlexLibraryNameById(libraryId, cancellationToken);

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

