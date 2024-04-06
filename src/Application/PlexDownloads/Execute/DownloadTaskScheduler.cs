using System.Text.Json;
using Application.Contracts;
using Logging.Interface;
using Quartz;

namespace PlexRipper.Application;

public class DownloadTaskScheduler : IDownloadTaskScheduler
{
    private readonly ILog _log;
    private readonly IScheduler _scheduler;

    public DownloadTaskScheduler(ILog log, IScheduler scheduler)
    {
        _log = log;
        _scheduler = scheduler;
    }

    public async Task<Result> StartDownloadTaskJob(DownloadTaskKey downloadTaskKey)
    {
        if (!downloadTaskKey.IsValid)
            return ResultExtensions.IsInvalidId(nameof(DownloadTaskKey), downloadTaskKey.Id).LogWarning();

        var jobKey = DownloadJob.GetJobKey(downloadTaskKey.Id);
        if (await _scheduler.IsJobRunning(jobKey))
            return Result.Fail($"{nameof(DownloadJob)} with {jobKey} already exists").LogWarning();

        var job = JobBuilder.Create<DownloadJob>()
            .UsingJobData(DownloadJob.DownloadTaskIdParameter, JsonSerializer.Serialize(downloadTaskKey))
            .WithIdentity(jobKey)
            .Build();

        var trigger = TriggerBuilder.Create()
            .WithIdentity($"{jobKey.Name}_trigger", jobKey.Group)
            .StartNow()
            .Build();

        await _scheduler.ScheduleJob(job, trigger);

        return Result.Ok();
    }

    public async Task<Result> StopDownloadTaskJob(DownloadTaskKey downloadTaskKey, CancellationToken cancellationToken = default)
    {
        if (!downloadTaskKey.IsValid)
            return ResultExtensions.IsInvalidId(nameof(DownloadTaskKey), downloadTaskKey.Id).LogWarning();

        _log.Information("Stopping DownloadClient for DownloadTaskId {DownloadTaskId}", downloadTaskKey);

        var jobKey = DownloadJob.GetJobKey(downloadTaskKey.Id);
        if (!await _scheduler.IsJobRunning(jobKey))
            return Result.Fail($"{nameof(DownloadJob)} with {jobKey} cannot be stopped because it is not running").LogWarning();

        var stopResult = await _scheduler.StopJob(jobKey);
        if (!stopResult)
            return Result.Fail($"Failed to stop {nameof(DownloadTaskGeneric)} with id {downloadTaskKey}");

        await AwaitDownloadTaskJob(downloadTaskKey.Id, cancellationToken);

        return Result.Ok();
    }

    public async Task AwaitDownloadTaskJob(Guid downloadTaskId, CancellationToken cancellationToken = default)
    {
        var jobKey = DownloadJob.GetJobKey(downloadTaskId);
        if (!await _scheduler.IsJobRunning(jobKey))
            return;

        await _scheduler.AwaitJobRunning(jobKey, cancellationToken);
    }

    public Task<bool> IsDownloading(DownloadTaskKey downloadTaskKey)
    {
        var jobKey = DownloadJob.GetJobKey(downloadTaskKey.Id);
        return _scheduler.IsJobRunningAsync(jobKey);
    }

    public async Task<bool> IsServerDownloading(int plexServerId)
    {
        var data = await _scheduler.GetRunningJobDataMaps(typeof(DownloadJob));
        return data.Select(x => x.GetJsonValue<DownloadTaskKey>(DownloadJob.DownloadTaskIdParameter).PlexServerId).Any(x => x == plexServerId);
    }
}