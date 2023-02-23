using BackgroundServices.Contracts;
using Quartz;
using WebAPI.Contracts;

namespace BackgroundServices.Listeners;

public class AllJobListener : IAllJobListener
{
    private readonly ISignalRService _signalRService;

    public string Name { get; } = nameof(AllJobListener);

    public AllJobListener(ISignalRService signalRService)
    {
        _signalRService = signalRService;
    }

    public async Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken = new())
    {
        var key = context.JobDetail.Key;
        var data = context.JobDetail.JobDataMap.WrappedMap.FirstOrDefault();

        var update = new JobStatusUpdate()
        {
            Id = context.FireInstanceId,
            JobName = key.Name,
            JobGroup = key.Group,
            JobRuntime = context.JobRunTime,
            JobStartTime = context.FireTimeUtc.UtcDateTime,
            Status = JobStatus.Running,
            PrimaryKey = data.Key ?? string.Empty,
            PrimaryKeyValue = Convert.ToInt32(data.Value ?? 0),
        };
        await _signalRService.SendJobStatusUpdateAsync(update);
    }

    public Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken cancellationToken = new())
    {
        return Task.CompletedTask;
    }

    public async Task JobWasExecuted(IJobExecutionContext context, JobExecutionException jobException, CancellationToken cancellationToken = new())
    {
        var key = context.JobDetail.Key;
        var update = new JobStatusUpdate()
        {
            Id = context.FireInstanceId,
            JobName = key.Name,
            JobGroup = key.Group,
            JobRuntime = context.JobRunTime,
            JobStartTime = context.FireTimeUtc.UtcDateTime,
            Status = JobStatus.Completed,
        };

        await _signalRService.SendJobStatusUpdateAsync(update);
    }
}