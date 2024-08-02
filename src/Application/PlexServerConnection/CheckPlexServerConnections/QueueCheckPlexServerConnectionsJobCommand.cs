using System.Text.Json;
using Data.Contracts;
using FluentValidation;
using Logging.Interface;
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
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;
    private readonly IScheduler _scheduler;

    public QueueCheckPlexServerConnectionsJobHandler(ILog log, IPlexRipperDbContext dbContext, IScheduler scheduler)
    {
        _log = log;
        _dbContext = dbContext;
        _scheduler = scheduler;
    }

    public async Task<Result<JobKey>> Handle(
        QueueCheckPlexServerConnectionsJobCommand command,
        CancellationToken cancellationToken
    )
    {
        var ids = JsonSerializer.Serialize(command.PlexServerIds);
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

        return Result.Ok(key);
    }
}
