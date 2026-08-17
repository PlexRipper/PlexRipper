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
