namespace Reaparr.Application;

/// <summary>Provides the typed Quartz execution boundary for every Reaparr job.</summary>
public static class JobExecutionContextExtensions
{
    public static void SetResult(this IJobExecutionContext context, JobStatus status, ResultBase result) =>
        context.SetResult(status, result.Errors.FirstOrDefault()?.Message);

    public static void SetResult(this IJobExecutionContext context, JobStatus status, string? errorSummary = null) =>
        context.Result = new BackgroundJobResult(status, errorSummary);

    public static Result<TPayload> GetRequiredPayload<TPayload>(this IJobExecutionContext context) =>
        Result.Try(() =>
        {
            var payload = context.MergedJobDataMap.GetPayload<TPayload>();
            return payload is null
                ? Result.Fail<TPayload>($"Missing required {typeof(TPayload).Name}")
                : Result.Ok(payload);
        });
}
