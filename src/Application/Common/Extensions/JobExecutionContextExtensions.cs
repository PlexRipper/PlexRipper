using System.Text.Json;
using Application.Contracts;
using Logging.Interface;
using Quartz;

namespace PlexRipper.Application;

public static class JobExecutionContextExtensions
{
    private static ILog _log = LogManager.CreateLogInstance(typeof(JobExecutionContextExtensions));

    /// <summary>
    /// Converts the <see cref="IJobExecutionContext"/> to a <see cref="JobStatusUpdate{T}"/>.
    /// </summary>
    /// <param name="context"> The <see cref="IJobExecutionContext"/> to convert. </param>
    /// <param name="status"> The Quartz <see cref="JobStatus"/> to set. </param>
    /// <returns></returns>
    /// <exception cref="Exception"> Thrown when the job type is unknown. </exception>
    public static JobStatusUpdate<string> ToJobStatusUpdate(this IJobExecutionContext context, JobStatus status)
    {
        var key = context.JobDetail.Key;
        var dataMap = context.JobDetail.JobDataMap;

        var jobType = JobStatusUpdateMapper.ToJobType(key.Group);

        var jsonString = string.Empty;
        switch (jobType)
        {
            case JobTypes.CheckAllConnectionsStatusByPlexServerJob:
                break;
            case JobTypes.DownloadJob:
                jsonString = dataMap.GetString(DownloadJob.DownloadTaskIdParameter) ?? string.Empty;
                break;
            case JobTypes.SyncServerMediaJob:
                jsonString = ToJsonString(
                    new SyncServerMediaJobUpdateDTO
                    {
                        PlexServerId = dataMap.GetIntValue(SyncServerMediaJob.PlexServerIdParameter),
                        ForceSync = dataMap.GetBooleanValue(SyncServerMediaJob.ForceSyncParameter),
                    }
                );
                break;
            case JobTypes.InspectPlexServerJob:
                jsonString = ToJsonString(dataMap.GetIntListValue(InspectPlexServerJob.PlexServerIdsParameter));
                break;

            case JobTypes.FileMergeJob:
                jsonString = ToJsonString(dataMap.GetJsonValue<DownloadTaskKey>(FileMergeJob.DownloadTaskIdParameter));
                break;

            default:
                throw new Exception($"Unknown job type: {jobType}");
        }

        return new JobStatusUpdate<string>(
            jobType,
            status,
            jsonString,
            context.FireInstanceId,
            context.FireTimeUtc.UtcDateTime
        );

        string ToJsonString<T>(T value)
        {
            try
            {
                return value is null
                    ? string.Empty
                    : JsonSerializer.Serialize(value, DefaultJsonSerializerOptions.ConfigStandard);
            }
            catch (Exception e)
            {
                _log.Error(e);
                throw;
            }
        }
    }
}
