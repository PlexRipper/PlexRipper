namespace Reaparr.Application;

public record QueueInspectPlexServerJobCommand(List<int> PlexServerIds) : ICommand<Result>;

public class QueueInspectPlexServerJobCommandValidator : AbstractValidator<QueueInspectPlexServerJobCommand>
{
    public QueueInspectPlexServerJobCommandValidator()
    {
        RuleFor(x => x.PlexServerIds).NotNull().NotEmpty();
    }
}

public class QueueInspectPlexServerJobCommandHandler : ICommandHandler<QueueInspectPlexServerJobCommand, Result>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly IScheduler _scheduler;

    public QueueInspectPlexServerJobCommandHandler(ILogger log, IReaparrDbContext dbContext, IScheduler scheduler)
    {
        _log = log.ForContext<QueueInspectPlexServerJobCommandHandler>();
        _dbContext = dbContext;
        _scheduler = scheduler;
    }

    public async Task<Result> ExecuteAsync(
        QueueInspectPlexServerJobCommand command,
        CancellationToken cancellationToken
    )
    {
        var plexServerIds = command.PlexServerIds;

        var plexServers = await _dbContext.PlexServers
            .IgnoreIsEnabledFilter()
            .Where(x => plexServerIds.Contains(x.Id))
            .ToListAsync(cancellationToken);

        if (!plexServers.Any())
        {
            _log.Here().Warning("No Plex servers found for {PlexServerIds} to queue for InspectPlexServerJob", plexServerIds);
            return Result.Fail("No Plex servers were found for the requested ids").LogWarning();
        }

        var disabledServerIds = plexServers.Where(x => !x.IsEnabled).Select(x => x.Id).ToList();
        if (disabledServerIds.Any())
        {
            _log.Here().Warning("Skipping disabled PlexServerIds when queueing InspectPlexServerJob: {PlexServerIds}", disabledServerIds);
        }

        var foundServerIds = plexServers.Select(x => x.Id).ToHashSet();
        var notFoundServerIds = plexServerIds.Where(x => !foundServerIds.Contains(x)).ToList();
        if (notFoundServerIds.Any())
        {
            _log.Here().Warning("No Plex servers found for ids {PlexServerIds}", notFoundServerIds);
        }

        var list = await _scheduler.GetRunningJobDataMaps(typeof(InspectPlexServerJob));
        var runningPlexServerIds = list.SelectMany(x => x.GetIntListValue(InspectPlexServerJob.PlexServerIdsParameter))
            .ToList();

        var enabledServerIds = plexServers.Where(x => x.IsEnabled).Select(x => x.Id).ToList();
        var alreadyRunning = enabledServerIds.Intersect(runningPlexServerIds).ToList();
        foreach (var i in alreadyRunning)
        {
            var plexServerName = await _dbContext.GetPlexServerNameById(i);
            _log.Here()
                .Error(
                    "Job {InspectPlexServerJobName} is already running for server: {PlexServerIdName} with id: {PlexServerId}",
                    nameof(InspectPlexServerJob),
                    plexServerName,
                    i
                );
        }

        var queuedServerIds = enabledServerIds.Where(x => !alreadyRunning.Contains(x)).ToList();
        if (!queuedServerIds.Any())
        {
            _log.Here().Warning("No enabled Plex servers available to queue for InspectPlexServerJob from requested ids {PlexServerIds}", plexServerIds);
            return Result.Fail("No enabled Plex servers were found for the requested ids").LogWarning();
        }

        var jobKey = InspectPlexServerJob.GetJobKey();
        var job = JobBuilder
            .Create<InspectPlexServerJob>()
            .UsingJobData(
                InspectPlexServerJob.PlexServerIdsParameter,
                JsonSerializer.Serialize(queuedServerIds, DefaultJsonSerializerOptions.ConfigStandard)
            )
            .WithIdentity(jobKey)
            .Build();

        var trigger = TriggerBuilder
            .Create()
            .WithIdentity($"{jobKey.Name}_trigger", jobKey.Group)
            .ForJob(job)
            .StartNow()
            .Build();

        await _scheduler.ScheduleJobAsync(job, trigger, cancellationToken);

        return Result.Ok();
    }
}