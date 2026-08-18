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

        var baseException = exception.GetBaseException();
        return baseException is OperationCanceledException
            ? new(JobStatus.Cancelled, baseException.Message)
            : new(JobStatus.Failed, baseException.Message);
    }
}
