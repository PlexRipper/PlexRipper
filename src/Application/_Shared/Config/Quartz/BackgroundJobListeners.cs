using Quartz.Listener;

namespace Reaparr.Application;

public sealed record BackgroundJobResult(JobStatus Status, string? ErrorSummary = null);

public sealed record BackgroundJobTerminalOutcome(JobStatus Status, string? ErrorSummary)
{
    public static BackgroundJobTerminalOutcome From(IJobExecutionContext context, JobExecutionException? exception)
    {
        if (context.Result is BackgroundJobResult result)
            return new(result.Status, result.ErrorSummary);

        if (exception is null)
            return new(JobStatus.Completed, null);

        return exception.InnerException is OperationCanceledException
            ? new(JobStatus.Cancelled, exception.Message)
            : new(JobStatus.Failed, exception.GetBaseException().Message);
    }
}

public sealed class AllJobListener : IJobListener
{
    private readonly ILogger _log;
    private readonly IProgressHubService _progressHubService;

    public AllJobListener(ILogger log, IProgressHubService progressHubService)
    {
        _log = log.ForContext<AllJobListener>();
        _progressHubService = progressHubService;
    }

    public string Name => nameof(AllJobListener);

    public Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken cancellationToken = default) =>
        Publish(context, JobStatus.Cancelled);

    public Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken = default) =>
        Publish(context, JobStatus.Started);

    public Task JobWasExecuted(
        IJobExecutionContext context,
        JobExecutionException? jobException,
        CancellationToken cancellationToken = default
    ) => Publish(context, BackgroundJobTerminalOutcome.From(context, jobException).Status);

    private async Task Publish(IJobExecutionContext context, JobStatus status)
    {
        var result = await Result.Try(async Task () =>
        {
            var update = new JobStatusUpdate<string>(
                JobStatusUpdateMapper.ToJobType(context.JobDetail.Key.Group),
                status,
                context.GetPayloadAsJson(),
                context.JobDetail.Key.Name,
                context.FireTimeUtc.UtcDateTime
            );
            await _progressHubService.SendJobStatusUpdateAsync(update);
        });

        if (result.IsFailed && !result.IsCancelled)
        {
            result.WithError($"Failed to publish Quartz lifecycle event for {context.JobDetail.Key}");
            result.LogError();
        }
    }
}

public sealed class ReaparrSchedulerListener(ILogger log) : SchedulerListenerSupport
{
    private readonly ILogger _log = log.ForContext<ReaparrSchedulerListener>();

    public override Task SchedulerError(
        string message,
        SchedulerException cause,
        CancellationToken cancellationToken = default
    )
    {
        _log.Here().Error(cause, "Quartz scheduler error: {Message}", message);
        return Task.CompletedTask;
    }
}
