using Quartz.Impl.Matchers;
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
        PublishSafely(context, JobStatus.Cancelled, cancellationToken);

    public Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken = default) =>
        PublishSafely(context, JobStatus.Started, cancellationToken);

    public Task JobWasExecuted(
        IJobExecutionContext context,
        JobExecutionException? jobException,
        CancellationToken cancellationToken = default
    ) => PublishSafely(context, BackgroundJobTerminalOutcome.From(context, jobException).Status, cancellationToken);

    private async Task PublishSafely(
        IJobExecutionContext context,
        JobStatus status,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var update = new JobStatusUpdate<string>(
                JobStatusUpdateMapper.ToJobType(context.JobDetail.Key.Group),
                status,
                string.Empty,
                context.JobDetail.Key.Name,
                context.FireTimeUtc.UtcDateTime
            );
            await _progressHubService.SendJobStatusUpdateAsync(update);
        }
        catch (Exception exception)
            when (exception is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            _log.Here()
                .Error(exception, "Failed to publish Quartz lifecycle event for {JobKey}", context.JobDetail.Key);
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
