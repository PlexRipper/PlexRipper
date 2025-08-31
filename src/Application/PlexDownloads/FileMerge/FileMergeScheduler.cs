using System.Text.Json;
using Quartz;
using Reaparr.FileSystem.Contracts;
using Reaparr.Logging;
using Serilog;

namespace Reaparr.Application;

public class FileMergeScheduler : IFileMergeScheduler
{
    private readonly Serilog.ILogger _log;
    private readonly IScheduler _scheduler;

    public FileMergeScheduler(ILogger log, IScheduler scheduler)
    {
        _log = log.ForContext<FileMergeScheduler>();
        _scheduler = scheduler;
    }

    /// <summary>
    /// Should only be called by the <see cref="FileMergeQueue"/> to start a new <see cref="FileMergeJob"/>.
    /// </summary>
    /// <param name="downloadTaskKey"> The key of the <see cref="DownloadTaskGeneric"/> to merge/move. </param>
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

        _log.Here().Information(
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

    public async Task<bool> IsAnyFileMergeJobRunning() =>
        (await _scheduler.GetRunningJobDataMaps(typeof(FileMergeJob))).Any();
}
