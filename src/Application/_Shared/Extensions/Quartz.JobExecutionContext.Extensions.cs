namespace Reaparr.Application;

public static partial class QuartzExtensions
{
    public static void SetResult(this IJobExecutionContext context, JobStatus status, ResultBase result) =>
        context.SetResult(status, result.Errors.FirstOrDefault()?.Message);

    public static void SetResult(this IJobExecutionContext context, JobStatus status, string? errorSummary = null) =>
        context.Result = new BackgroundJobResult(status, errorSummary);

    public static JobTypes GetJobType(this IJobExecutionContext context) =>
        JobStatusUpdateMapper.ToJobType(context.JobDetail.Key.Group);

    public static Result<TPayload> GetRequiredPayload<TPayload>(this IJobExecutionContext context) =>
        Result.Try(() =>
        {
            var payload = context.MergedJobDataMap.GetPayload<TPayload>();
            return payload is null
                ? Result.Fail<TPayload>($"Missing required {typeof(TPayload).Name}")
                : Result.Ok(payload);
        });

    public static string GetPayloadAsJson(this IJobExecutionContext context)
    {
        var defaultJson = "{}";
        switch (context.GetJobType())
        {
            case JobTypes.DownloadJob:
                return JsonSerializer.Serialize(
                    context.MergedJobDataMap.GetPayload<DownloadJobPayload>(),
                    DefaultJsonSerializerOptions.ConfigStandard
                );
            case JobTypes.MoveDownloadFileJob:
                return JsonSerializer.Serialize(
                    context.MergedJobDataMap.GetPayload<MoveDownloadFileJobPayload>(),
                    DefaultJsonSerializerOptions.ConfigStandard
                );
            case JobTypes.InspectPlexServerJob:
                return JsonSerializer.Serialize(
                    context.MergedJobDataMap.GetPayload<InspectPlexServerJobPayload>(),
                    DefaultJsonSerializerOptions.ConfigStandard
                );
            case JobTypes.LibrarySyncJob:
                return JsonSerializer.Serialize(
                    context.MergedJobDataMap.GetPayload<LibrarySyncJobPayload>(),
                    DefaultJsonSerializerOptions.ConfigStandard
                );
            case JobTypes.LibraryComparisonJob:
                return JsonSerializer.Serialize(
                    context.MergedJobDataMap.GetPayload<PlexLibraryComparisonJobPayload>(),
                    DefaultJsonSerializerOptions.ConfigStandard
                );
            // CRON jobs have no payload, so we return an empty JSON object.
            case JobTypes.MetadataSyncJob:
            case JobTypes.CheckForUpdateJob:
            case JobTypes.CheckAllConnectionsStatusByPlexServerJob:
            case JobTypes.CheckPlexLibrariesForUpdatesJob:
            case JobTypes.RefreshPlexAccountAccessJob:
                return defaultJson;
            case JobTypes.Unknown:
            case JobTypes.None:
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(context),
                    context.GetJobType(),
                    $"Unknown job type {context.GetJobType()}"
                );
        }
    }
}
