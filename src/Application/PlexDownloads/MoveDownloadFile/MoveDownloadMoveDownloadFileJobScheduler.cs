using System.Text.Json;
using Quartz;
using Reaparr.FileSystem.Contracts;

namespace Reaparr.Application;

public class MoveDownloadMoveDownloadFileJobScheduler : IMoveDownloadFileScheduler
{
    private readonly ILogger _log;
    private readonly IScheduler _scheduler;

    public MoveDownloadMoveDownloadFileJobScheduler(ILogger log, IScheduler scheduler)
    {
        _log = log.ForContext<MoveDownloadMoveDownloadFileJobScheduler>();
        _scheduler = scheduler;
    }

    /// <summary>
    /// Should only be called by the <see cref="MoveDownloadMoveDownloadFileJobQueue"/> to start a new <see cref="MoveDownloadFileJob"/>.
    /// </summary>
    /// <param name="downloadTaskKey"> The key of the <see cref="DownloadTaskGeneric"/> to merge/move. </param>
    public async Task<Result> StartMoveDownloadFileJob(DownloadTaskKey downloadTaskKey)
    {
        if (!downloadTaskKey.IsValid)
            return ResultExtensions.IsInvalidId(nameof(DownloadTaskKey), downloadTaskKey.Id).LogWarning();

        var jobKey = MoveDownloadFileJob.GetJobKey(downloadTaskKey.Id);
        if (await _scheduler.IsJobRunning(jobKey))
            return Result.Fail($"{nameof(MoveDownloadFileJob)} with {jobKey} already exists").LogWarning();

        var job = JobBuilder
            .Create<MoveDownloadFileJob>()
            .UsingJobData(MoveDownloadFileJob.DownloadTaskIdParameter, JsonSerializer.Serialize(downloadTaskKey))
            .WithIdentity(jobKey)
            .Build();

        var trigger = TriggerBuilder.Create().WithIdentity($"{jobKey.Name}_trigger", jobKey.Group).StartNow().Build();

        await _scheduler.ScheduleJob(job, trigger);

        return Result.Ok();
    }

    public async Task<Result> StopMoveDownloadFileJob(DownloadTaskKey downloadTaskKey)
    {
        if (!downloadTaskKey.IsValid)
            return ResultExtensions.IsInvalidId(nameof(DownloadTaskKey), downloadTaskKey.Id).LogWarning();

        _log.Here()
            .Information(
                "Stopping MoveDownloadJob for {NameOfDownloadFileTask)} with id: {FileTaskId}",
                nameof(DownloadTaskKey),
                downloadTaskKey.Id
            );

        var jobKey = MoveDownloadFileJob.GetJobKey(downloadTaskKey.Id);
        if (!await _scheduler.IsJobRunning(jobKey))
        {
            return Result
                .Fail($"{nameof(MoveDownloadFileJob)} with {jobKey} cannot be stopped because it is not running")
                .LogWarning();
        }

        var wasStopped = await _scheduler.StopJob(jobKey);

        return !wasStopped
            ? Result.Fail($"Failed to stop {nameof(DownloadTaskKey)} with id {downloadTaskKey.Id}").LogError()
            : Result.Ok();
    }

    public async Task<bool> IsDownloadFileMoving(DownloadTaskKey downloadTaskKey) =>
        await _scheduler.IsJobRunningAsync(MoveDownloadFileJob.GetJobKey(downloadTaskKey.Id));

    public async Task<bool> IsAnyMoveDownloadFileJobRunning() =>
        (await _scheduler.GetRunningJobDataMaps(typeof(MoveDownloadFileJob))).Any();
}
