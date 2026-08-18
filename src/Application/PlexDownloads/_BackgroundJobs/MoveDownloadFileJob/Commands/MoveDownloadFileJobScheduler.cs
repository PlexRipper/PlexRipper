namespace Reaparr.Application;

public class MoveDownloadFileJobScheduler : IMoveDownloadFileScheduler
{
    private readonly ILogger _log;
    private readonly IScheduler _scheduler;

    public MoveDownloadFileJobScheduler(ILogger log, IScheduler scheduler)
    {
        _log = log.ForContext<MoveDownloadFileJobScheduler>();
        _scheduler = scheduler;
    }

    /// <summary>
    /// Should only be called by the <see cref="MoveDownloadFileJobQueue"/> to start a new <see cref="MoveDownloadFileJob"/>.
    /// </summary>
    public async Task<Result> StartMoveDownloadFileJob(
        DownloadTaskKey downloadTaskKey,
        CancellationToken cancellationToken
    )
    {
        if (!downloadTaskKey.IsValid)
            return ResultExtensions.IsInvalidId(nameof(DownloadTaskKey), downloadTaskKey.Id).LogWarning();

        var jobKey = MoveDownloadFileJob.GetJobKey(downloadTaskKey.Id);
        if (await _scheduler.IsJobRunning(jobKey, cancellationToken))
            return Result.Fail($"{nameof(MoveDownloadFileJob)} with {jobKey} already exists").LogWarning();

        var schedulingResult = await _scheduler.ExecuteJob<MoveDownloadFileJob, MoveDownloadFileJobPayload>(
            jobKey,
            new MoveDownloadFileJobPayload(downloadTaskKey),
            cancellationToken
        );
        schedulingResult.LogIfFailed();
        return schedulingResult.ToResult();
    }

    public async Task<Result> StopMoveDownloadFileJob(
        DownloadTaskKey downloadTaskKey,
        CancellationToken cancellationToken
    )
    {
        if (!downloadTaskKey.IsValid)
            return ResultExtensions.IsInvalidId(nameof(DownloadTaskKey), downloadTaskKey.Id).LogWarning();

        _log.Here()
            .Information(
                "Stopping MoveDownloadJob for {NameOfDownloadFileTask} with id: {FileTaskId}",
                nameof(DownloadTaskKey),
                downloadTaskKey.Id
            );

        var jobKey = MoveDownloadFileJob.GetJobKey(downloadTaskKey.Id);
        if (!await _scheduler.IsJobRunning(jobKey, cancellationToken))
        {
            return Result
                .Fail($"{nameof(MoveDownloadFileJob)} with {jobKey} cannot be stopped because it is not running")
                .LogWarning();
        }

        var wasStopped = await _scheduler.Interrupt(jobKey, cancellationToken);

        return !wasStopped
            ? Result.Fail($"Failed to stop {nameof(DownloadTaskKey)} with id {downloadTaskKey.Id}").LogError()
            : Result.Ok();
    }

    public Task<bool> IsDownloadFileMoving(DownloadTaskKey downloadTaskKey, CancellationToken cancellationToken) =>
        _scheduler.IsJobRunning(MoveDownloadFileJob.GetJobKey(downloadTaskKey.Id), cancellationToken);

    public async Task<bool> IsAnyMoveDownloadFileJobRunning() =>
        (await _scheduler.GetCurrentlyExecutingJobs()).Any(x =>
            x.JobDetail.Key.Group == nameof(JobTypes.MoveDownloadFileJob)
        );

    public async Task<List<DownloadTaskKey>> GetCurrentlyMovingKeysByServer(int plexServerId)
    {
        var contexts = await _scheduler.GetCurrentlyExecutingJobs(CancellationToken.None);
        return contexts
            .Where(x => x.JobDetail.Key.Group == nameof(JobTypes.MoveDownloadFileJob))
            .Select(x => x.MergedJobDataMap.GetPayload<MoveDownloadFileJobPayload>()?.DownloadTaskKey)
            .OfType<DownloadTaskKey>()
            .Where(x => x.PlexServerId == plexServerId)
            .ToList();
    }
}
