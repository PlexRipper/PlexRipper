using Quartz;
using Riok.Mapperly.Abstractions;

namespace Application.Contracts;

[Mapper]
public static partial class JobStatusUpdateMapper
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

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    public static partial JobStatusUpdateDTO ToDTO(this JobStatusUpdate jobStatusUpdate);

    #endregion

    private static JobTypes ToJobType(string jobGroup) => Enum.TryParse<JobTypes>(jobGroup, out var jobType) ? jobType : JobTypes.Unknown;
}