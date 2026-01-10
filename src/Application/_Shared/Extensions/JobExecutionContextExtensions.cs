using System.Text.Json;
using Quartz;
using Reaparr.Application.Contracts;

namespace Reaparr.Application;

public static class JobExecutionContextExtensions
{
    private static readonly ILogger _log = LogFactory.Create(typeof(JobExecutionContextExtensions));

    /// <summary>
    /// Converts the <see cref="IJobExecutionContext"/> to a <see cref="JobStatusUpdate{T}"/>.
    /// </summary>
    /// <param name="context"> The <see cref="IJobExecutionContext"/> to convert. </param>
    /// <param name="status"> The Quartz <see cref="JobStatus"/> to set. </param>
    /// <returns></returns>
    /// <remarks>Some job types handle their own status updates directly via SignalR and will return empty JSON from this method.</remarks>
    public static JobStatusUpdate<string> ToJobStatusUpdate(this IJobExecutionContext context, JobStatus status)
    {
        var key = context.JobDetail.Key;
        var dataMap = context.JobDetail.JobDataMap;

        var jobType = JobStatusUpdateMapper.ToJobType(key.Group);

        var jsonString = string.Empty;
        switch (jobType)
        {
            // NOTE: This job sends its own updates to the client because the data is not passed in but determined dynamically while the job is running.
            case JobTypes.CheckAllConnectionsStatusByPlexServerJob:
                // Type is CheckAllConnectionStatusUpdateDTO, but it is not passed in here.
                break;
            case JobTypes.DownloadJob:
                jsonString = ToJsonString(
                    new DownloadJobUpdateDTO
                    {
                        Id = dataMap.GetJsonValue<DownloadTaskKey>(DownloadJob.DownloadTaskIdParameter)!,
                    }
                );
                break;
            case JobTypes.InspectPlexServerJob:
                jsonString = ToJsonString(
                    new InspectPlexServerJobUpdateDTO
                    {
                        PlexServerIds = dataMap.GetIntListValue(InspectPlexServerJob.PlexServerIdsParameter),
                    }
                );
                break;

            case JobTypes.MoveDownloadFileJob:
                jsonString = ToJsonString(
                    new MoveDownloadFileJobUpdateDTO
                    {
                        DownloadTaskId = dataMap.GetJsonValue<DownloadTaskKey>(
                            MoveDownloadFileJob.DownloadTaskIdParameter
                        )!,
                    }
                );
                break;

            // NOTE: LibrarySyncJob handles its own status updates via SignalR in LibrarySyncJobListener
            case JobTypes.LibrarySyncJob:
                break;

            default:
                jsonString = "{}";
                break;
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
                return JsonSerializer.Serialize(value, DefaultJsonSerializerOptions.ConfigStandard);
            }
            catch (Exception e)
            {
                _log.Here().ErrorResult(e);
                return "{}";
            }
        }
    }
}
