using System.Text.Json;
using Application.Contracts;
using Data.Contracts;
using FluentValidation;
using Logging.Interface;
using PlexRipper.Domain.Config;
using Quartz;

namespace PlexRipper.Application;

public record QueueInspectPlexServerJobCommand(List<int> PlexServerIds) : IRequest<Result>;

public class QueueInspectPlexServerJobCommandValidator : AbstractValidator<QueueInspectPlexServerJobCommand>
{
    public QueueInspectPlexServerJobCommandValidator()
    {
        RuleFor(x => x.PlexServerIds.Count).GreaterThan(0);
    }
}

public class QueueInspectPlexServerJobCommandHandler : IRequestHandler<QueueInspectPlexServerJobCommand, Result>
{
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;
    private readonly IScheduler _scheduler;

    public QueueInspectPlexServerJobCommandHandler(ILog log, IPlexRipperDbContext dbContext, IScheduler scheduler)
    {
        _log = log;
        _dbContext = dbContext;
        _scheduler = scheduler;
    }

    public async Task<Result> Handle(QueueInspectPlexServerJobCommand command, CancellationToken cancellationToken)
    {
        var plexServerIds = command.PlexServerIds;

        var plexServer = await _dbContext
            .PlexServers.Where(x => plexServerIds.Contains(x.Id))
            .ToListAsync(cancellationToken);

        var enabledServers = plexServer.Where(x => x.IsEnabled).ToList();
        var notEnabledServers = plexServer.Where(x => !x.IsEnabled).ToList();

        if (notEnabledServers.Any())
        {
            var result = notEnabledServers
                .Select(x =>
                    ResultExtensions
                        .ServerIsNotEnabled(x.Name, x.Id, nameof(QueueInspectPlexServerJobCommand))
                        .LogError()
                )
                .Merge();

            if (!enabledServers.Any())
                return result;
        }

        var list = await _scheduler.GetRunningJobDataMaps(typeof(InspectPlexServerJob));
        var runningPlexServerIds = list.SelectMany(x => x.GetIntListValue(InspectPlexServerJob.PlexServerIdsParameter))
            .ToList();

        var enabledServerIds = enabledServers.Select(x => x.Id).ToList();
        var alreadyRunning = enabledServerIds.Intersect(runningPlexServerIds).ToList();
        foreach (var i in alreadyRunning)
        {
            var plexServerName = await _dbContext.GetPlexServerNameById(i, cancellationToken);
            _log.Error(
                "Job {InspectPlexServerJobName} is already running for serverL {PlexServerIdName} with id: {PlexServerId}",
                nameof(InspectPlexServerJob),
                plexServerName,
                i
            );
        }

        var queuedServerIds = enabledServerIds.Where(x => !alreadyRunning.Contains(x)).ToList();

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
