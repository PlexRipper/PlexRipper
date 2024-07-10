using Quartz;

namespace Application.Contracts;

public static class JobStatusUpdateMapper
{
    #region FromScheduler

    public static JobStatusUpdate ToUpdate(this IJobExecutionContext context, JobStatus jobStatus)
    {
        var key = context.JobDetail.Key;
        var data = context.JobDetail.JobDataMap.WrappedMap.FirstOrDefault();

        return new JobStatusUpdate()
        {
            Id = context.FireInstanceId,
            JobName = key.Name,
            JobGroup = key.Group,
            JobRuntime = context.JobRunTime,
            JobStartTime = context.FireTimeUtc.UtcDateTime,
            Status = jobStatus,
            PrimaryKey = data.Key ?? string.Empty,
            PrimaryKeyValue = data.Value?.ToString() ?? string.Empty,
            JobType = ToJobType(key.Group),
        };
    }

    #endregion

    #region ToDTO

    public static JobStatusUpdateDTO ToDTO(this JobStatusUpdate jobStatusUpdate) => new()
    {
        Id = jobStatusUpdate.Id,
        JobName = jobStatusUpdate.JobName,
        JobGroup = jobStatusUpdate.JobGroup,
        JobRuntime = jobStatusUpdate.JobRuntime,
        JobStartTime = jobStatusUpdate.JobStartTime,
        Status = jobStatusUpdate.Status,
        PrimaryKey = jobStatusUpdate.PrimaryKey,
        PrimaryKeyValue = jobStatusUpdate.PrimaryKeyValue,
        JobType = jobStatusUpdate.JobType,
    };

    #endregion

    private static JobTypes ToJobType(string jobGroup) => Enum.TryParse<JobTypes>(jobGroup, out var jobType) ? jobType : JobTypes.Unknown;
}