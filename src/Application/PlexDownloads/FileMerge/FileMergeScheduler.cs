using System.Text.Json;
using FileSystem.Contracts;
using Logging.Interface;
using Quartz;

namespace PlexRipper.Application;

public class FileMergeScheduler : IFileMergeScheduler
{
    private readonly ILog _log;
    private readonly IScheduler _scheduler;

    public FileMergeScheduler(ILog log, IScheduler scheduler)
    {
        _log = log;
        _scheduler = scheduler;
    }

    public async Task<Result> StartFileMergeJob(DownloadTaskKey downloadTaskKey)
    {
        if (!downloadTaskKey.IsValid)
            return ResultExtensions.IsInvalidId(nameof(DownloadTaskKey), downloadTaskKey.Id).LogWarning();

        var jobKey = FileMergeJob.GetJobKey(downloadTaskKey.Id);
        if (await _scheduler.IsJobRunning(jobKey))
            return Result.Fail($"{nameof(FileMergeJob)} with {jobKey} already exists").LogWarning();

        var job = JobBuilder
            .Create<FileMergeJob>()
            .UsingJobData(FileMergeJob.DownloadTaskIdParameter, JsonSerializer.Serialize(downloadTaskKey))
            .WithIdentity(jobKey)
            .Build();

        var trigger = TriggerBuilder.Create().WithIdentity($"{jobKey.Name}_trigger", jobKey.Group).StartNow().Build();

        await _scheduler.ScheduleJob(job, trigger);

        return Result.Ok();
    }

    public async Task<Result> StopFileMergeJob(DownloadTaskKey downloadTaskKey)
    {
        if (!downloadTaskKey.IsValid)
            return ResultExtensions.IsInvalidId(nameof(DownloadTaskKey), downloadTaskKey.Id).LogWarning();

        _log.Information(
            "Stopping FileMergeJob for {NameOfDownloadFileTask)} with id: {FileTaskId}",
            nameof(DownloadTaskKey),
            downloadTaskKey.Id
        );

        var jobKey = FileMergeJob.GetJobKey(downloadTaskKey.Id);
        if (!await _scheduler.IsJobRunning(jobKey))
        {
            return Result
                .Fail($"{nameof(FileMergeJob)} with {jobKey} cannot be stopped because it is not running")
                .LogWarning();
        }

        var wasStopped = await _scheduler.StopJob(jobKey);

        return !wasStopped
            ? Result.Fail($"Failed to stop {nameof(DownloadTaskKey)} with id {downloadTaskKey.Id}").LogError()
            : Result.Ok();
    }

    public async Task<bool> IsDownloadTaskMerging(DownloadTaskKey downloadTaskKey) =>
        await _scheduler.IsJobRunningAsync(FileMergeJob.GetJobKey(downloadTaskKey.Id));
}
