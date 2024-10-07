using System.Text.Json;
using Data.Contracts;
using FluentValidation;
using Quartz;

namespace PlexRipper.Application;

public record QueueCheckPlexServerConnectionsJobCommand(List<int> PlexServerIds) : IRequest<Result<JobKey>>;

public class QueueCheckPlexServerConnectionsJobCommandValidator
    : AbstractValidator<QueueCheckPlexServerConnectionsJobCommand>
{
    public QueueCheckPlexServerConnectionsJobCommandValidator()
    {
        RuleFor(x => x.PlexServerIds.Count).GreaterThan(0);
    }
}

public class QueueCheckPlexServerConnectionsJobHandler
    : IRequestHandler<QueueCheckPlexServerConnectionsJobCommand, Result<JobKey>>
{
    private readonly IPlexRipperDbContext _dbContext;
    private readonly IScheduler _scheduler;

    public QueueCheckPlexServerConnectionsJobHandler(IPlexRipperDbContext dbContext, IScheduler scheduler)
    {
        _dbContext = dbContext;
        _scheduler = scheduler;
    }

    public async Task<Result<JobKey>> Handle(
        QueueCheckPlexServerConnectionsJobCommand command,
        CancellationToken cancellationToken
    )
    {
        var plexServerIds = command.PlexServerIds;
        var enabledIds = _dbContext
            .PlexServers.Where(x => x.IsEnabled && plexServerIds.Contains(x.Id))
            .Select(x => x.Id)
            .ToList();

        if (plexServerIds.Count != enabledIds.Count)
        {
            var failedResults = new List<Result>();
            foreach (var disabledServerId in plexServerIds.Except(enabledIds))
            {
                var plexServerName = await _dbContext.GetPlexServerNameById(disabledServerId, cancellationToken);

                failedResults.Add(
                    ResultExtensions.ServerIsNotEnabled(
                        plexServerName,
                        disabledServerId,
                        nameof(QueueCheckPlexServerConnectionsJobCommand)
                    )
                );
            }

            if (failedResults.Any())
                return failedResults.Merge().LogError();
        }

        if (!enabledIds.Any())
        {
            return ResultExtensions.Create403ForbiddenResult(
                $"None of the given PlexServerIds: {plexServerIds} are enabled"
            );
        }

        var ids = JsonSerializer.Serialize(enabledIds);
        var key = CheckPlexServerConnectionsJob.GetJobKey();

        var job = JobBuilder
            .Create<CheckPlexServerConnectionsJob>()
            .UsingJobData(CheckPlexServerConnectionsJob.PlexServerIdsParameter, ids)
            .WithIdentity(key)
            .Build();

        var trigger = TriggerBuilder
            .Create()
            .WithIdentity($"{key.Name}_trigger", key.Group)
            .ForJob(job)
            .StartNow()
            .Build();

        await _scheduler.ScheduleJob(job, trigger, cancellationToken);

        return key;
    }
}
