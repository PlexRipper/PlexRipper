using Application.Contracts;
using Logging.Interface;
using Quartz;

namespace PlexRipper.Application;

public class DownloadTaskScheduler : IDownloadTaskScheduler
{
    private readonly ILog _log;
    private readonly IScheduler _scheduler;

    protected JobKey DefaultJobKey => new($"DownloadTaskId_", nameof(DownloadTaskScheduler));

    public DownloadTaskScheduler(ILog log, IScheduler scheduler)
    {
        _log = log;
        _scheduler = scheduler;
    }

    public async Task<Result> StartDownloadTaskJob(int downloadTaskId, int plexServerId)
    {
        if (downloadTaskId <= 0)
            return ResultExtensions.IsInvalidId(nameof(downloadTaskId), downloadTaskId).LogWarning();

        var jobKey = DownloadJob.GetJobKey(downloadTaskId);
        if (await _scheduler.IsJobRunning(jobKey))
            return Result.Fail($"{nameof(DownloadJob)} with {jobKey} already exists").LogWarning();

        var job = JobBuilder.Create<DownloadJob>()
            .UsingJobData(DownloadJob.DownloadTaskIdParameter, downloadTaskId)
            .UsingJobData(DownloadJob.PlexServerIdParameter, plexServerId)
            .WithIdentity(jobKey)
            .Build();

        var trigger = TriggerBuilder.Create()
            .WithIdentity($"{jobKey.Name}_trigger", jobKey.Group)
            .StartNow()
            .Build();

        await _scheduler.ScheduleJob(job, trigger);

        return Result.Ok();
    }

    public async Task<Result> StopDownloadTaskJob(int downloadTaskId)
    {
        if (downloadTaskId <= 0)
            return ResultExtensions.IsInvalidId(nameof(downloadTaskId), downloadTaskId).LogWarning();

        _log.Information("Stopping DownloadClient for DownloadTaskId {DownloadTaskId}", downloadTaskId);

        var jobKey = DownloadJob.GetJobKey(downloadTaskId);
        if (!await _scheduler.IsJobRunning(jobKey))
            return Result.Fail($"{nameof(DownloadJob)} with {jobKey} cannot be stopped because it is not running").LogWarning();

        return Result.OkIf(await _scheduler.StopJob(jobKey), $"Failed to stop {nameof(DownloadTask)} with id {downloadTaskId}");
    }

    public Task AwaitDownloadTaskJob(int downloadTaskId, CancellationToken cancellationToken = default)
    {
        var jobKey = DownloadJob.GetJobKey(downloadTaskId);
        return _scheduler.AwaitJobRunning(jobKey, cancellationToken);
    }

    public Task<bool> IsDownloading(int downloadTaskId)
    {
        var jobKey = DownloadJob.GetJobKey(downloadTaskId);
        return _scheduler.IsJobRunningAsync(jobKey);
    }

    public async Task<bool> IsServerDownloading(int plexServerId)
    {
        var data = await _scheduler.GetRunningJobDataMaps(typeof(DownloadJob));
        return data.Select(x => x.GetIntValue(DownloadJob.PlexServerIdParameter)).ToList().Contains(plexServerId);
    }
}