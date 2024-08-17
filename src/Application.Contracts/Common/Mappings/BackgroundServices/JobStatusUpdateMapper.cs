using Quartz;

namespace Application.Contracts;

public static class JobStatusUpdateMapper
{
    #region FromScheduler

    public static JobStatusUpdate ToUpdate(this IJobExecutionContext context, JobStatus jobStatus)
    {
        var key = context.JobDetail.Key;
        var data = context.JobDetail.JobDataMap.WrappedMap.FirstOrDefault();

        return new JobStatusUpdate<string>(
            ToJobType(key.Group),
            jobStatus,
            data.Value?.ToString() ?? string.Empty,
            context.FireInstanceId,
            context.FireTimeUtc.UtcDateTime
        );
    }

    #endregion

    #region ToDTO

    public static JobStatusUpdateDTO ToDTO(this JobStatusUpdate jobStatusUpdate) =>
        new()
        {
            Id = jobStatusUpdate.Id,
            JobStartTime = jobStatusUpdate.JobStartTime,
            Status = jobStatusUpdate.Status,
            JobType = jobStatusUpdate.JobType,
        };

    public static JobStatusUpdateDTO<T> ToDTO<T>(this JobStatusUpdate<T> jobStatusUpdate)
        where T : class =>
        new()
        {
            Id = jobStatusUpdate.Id,
            JobStartTime = jobStatusUpdate.JobStartTime,
            Status = jobStatusUpdate.Status,
            JobType = jobStatusUpdate.JobType,
            Data = jobStatusUpdate.Data,
        };

    #endregion

    private static JobTypes ToJobType(string jobGroup) =>
        Enum.TryParse<JobTypes>(jobGroup, out var jobType)
            ? jobType
            : throw new Exception($"ToJobType => Unknown job type: {jobGroup}");
}
