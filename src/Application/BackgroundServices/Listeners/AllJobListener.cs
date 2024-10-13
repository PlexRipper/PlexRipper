using Application.Contracts;
using PlexRipper.Application.Jobs;
using Quartz;

namespace PlexRipper.Application;

public class AllJobListener : IAllJobListener
{
    private readonly ISignalRService _signalRService;

    public string Name => nameof(AllJobListener);

    public AllJobListener(ISignalRService signalRService)
    {
        _signalRService = signalRService;
    }

    public async Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken = new())
    {
        // Source: https://www.quartz-scheduler.net/documentation/quartz-3.x/tutorial/trigger-and-job-listeners.html
        // Make sure your trigger and job listeners never throw an exception (use a try-catch) and that they can handle internal problems. Jobs can get stuck after Quartz is unable to determine whether required logic in listener was completed successfully when listener notification failed.
        try
        {
            await SendJobExecutionContextAsync(context, JobStatus.Started);
        }
        catch (Exception e)
        {
            Result.Fail(new ExceptionalError(e)).LogError();
        }
    }

    public async Task JobWasExecuted(
        IJobExecutionContext context,
        JobExecutionException? jobException,
        CancellationToken cancellationToken = new()
    )
    {
        // Source: https://www.quartz-scheduler.net/documentation/quartz-3.x/tutorial/trigger-and-job-listeners.html
        // Make sure your trigger and job listeners never throw an exception (use a try-catch) and that they can handle internal problems. Jobs can get stuck after Quartz is unable to determine whether required logic in listener was completed successfully when listener notification failed.
        try
        {
            await SendJobExecutionContextAsync(context, JobStatus.Completed);
        }
        catch (Exception e)
        {
            Result.Fail(new ExceptionalError(e)).LogError();
        }
    }

    public Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken cancellationToken = new()) =>
        Task.CompletedTask;

    public async Task SendJobExecutionContextAsync(IJobExecutionContext context, JobStatus status)
    {
        var key = context.JobDetail.Key;
        var dataMap = context.JobDetail.JobDataMap;
        var data = dataMap.WrappedMap.FirstOrDefault();

        var jobType = JobStatusUpdateMapper.ToJobType(key.Group);

        var result = new JobStatusUpdate(jobType, status, context.FireInstanceId, context.FireTimeUtc.UtcDateTime);

        switch (jobType)
        {
            case JobTypes.CheckAllConnectionsStatusByPlexServerJob:
                break;
            case JobTypes.DownloadJob:
                await _signalRService.SendJobStatusUpdateAsync(
                    new JobStatusUpdate<DownloadTaskKey>(
                        result,
                        dataMap.GetJsonValue<DownloadTaskKey>(DownloadJob.DownloadTaskIdParameter)
                    )
                );
                break;
            case JobTypes.SyncServerMediaJob:
                await _signalRService.SendJobStatusUpdateAsync(
                    new JobStatusUpdate<SyncServerMediaJobUpdateDTO>(
                        result,
                        new SyncServerMediaJobUpdateDTO()
                        {
                            PlexServerId = dataMap.GetIntValue(SyncServerMediaJob.PlexServerIdParameter),
                            ForceSync = dataMap.GetBooleanValue(SyncServerMediaJob.ForceSyncParameter),
                        }
                    )
                );
                return;
            case JobTypes.InspectPlexServerJob:
                await _signalRService.SendJobStatusUpdateAsync(
                    new JobStatusUpdate<List<int>>(
                        result,
                        dataMap.GetIntListValue(InspectPlexServerJob.PlexServerIdsParameter)
                    )
                );
                return;
            case JobTypes.FileMergeJob:
                await _signalRService.SendJobStatusUpdateAsync(
                    new JobStatusUpdate<List<int>>(result, [dataMap.GetIntValue(FileMergeJob.FileTaskId)])
                );
                return;
            default:
                throw new ArgumentOutOfRangeException($"Add a new case for the new job type {jobType}");
        }

        var value = data.Value?.ToString() ?? string.Empty;
        await _signalRService.SendJobStatusUpdateAsync(new JobStatusUpdate<string>(result, value));
    }
}
