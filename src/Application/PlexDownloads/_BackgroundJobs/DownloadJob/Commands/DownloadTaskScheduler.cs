namespace Reaparr.Application;

public class DownloadTaskScheduler : IDownloadTaskScheduler
{
    private readonly ILogger _log;
    private readonly IScheduler _scheduler;

    public DownloadTaskScheduler(ILogger log, IScheduler scheduler)
    {
        _log = log.ForContext<DownloadTaskScheduler>();
        _scheduler = scheduler;
    }

    public async Task<Result> StartDownloadTaskJob(
        DownloadTaskKey downloadTaskKey,
        CancellationToken cancellationToken = default
    )
    {
        if (!downloadTaskKey.IsValid)
            return ResultExtensions.IsInvalidId(nameof(DownloadTaskKey), downloadTaskKey.Id).LogWarning();

        return await Result.Try(async Task<Result> () =>
        {
            var jobKey = DownloadJob.GetJobKey(downloadTaskKey.Id);
            if (await _scheduler.IsJobRunning(jobKey, cancellationToken) || await _scheduler.IsQueued(jobKey))
            {
                _log.Here().Debug("{DownloadJobName} with {JobKey} is already scheduled", nameof(DownloadJob), jobKey);
                return Result.Ok();
            }

            var schedulingResult = await _scheduler.ExecuteJob<DownloadJob, DownloadJobPayload>(
                jobKey,
                new DownloadJobPayload(downloadTaskKey),
                cancellationToken
            );

            schedulingResult.LogIfFailed();
            return schedulingResult.ToResult();
        });
    }

    public async Task<Result> StopDownloadTaskJob(
        DownloadTaskKey downloadTaskKey,
        CancellationToken cancellationToken = default,
        bool waitForCompletion = true
    )
    {
        if (!downloadTaskKey.IsValid)
            return ResultExtensions.IsInvalidId(nameof(DownloadTaskKey), downloadTaskKey.Id).LogWarning();

        return await Result.Try(async Task<Result> () =>
        {
            _log.Here().Information("Stopping DownloadClient for DownloadTaskId {DownloadTaskId}", downloadTaskKey);

            var jobKey = DownloadJob.GetJobKey(downloadTaskKey.Id);
            var isRunning = await _scheduler.IsJobRunning(jobKey, cancellationToken);
            var isQueued = await _scheduler.IsQueued(jobKey);
            if (!isRunning && !isQueued)
            {
                return Result
                    .Fail($"{nameof(DownloadJob)} with {jobKey} cannot be stopped because it is not scheduled")
                    .LogWarning();
            }

            if (isQueued && !isRunning)
                return await _scheduler.DeleteBatchJobs([jobKey], cancellationToken);

            var stopResult = await _scheduler.Interrupt(jobKey, cancellationToken);
            if (!stopResult)
                return Result
                    .Fail($"Failed to stop {nameof(DownloadTaskGeneric)} with id {downloadTaskKey}")
                    .LogError();

            if (waitForCompletion)
            {
                await AwaitDownloadTaskJob(downloadTaskKey.Id, cancellationToken);
            }

            return Result.Ok();
        });
    }

    public async Task AwaitDownloadTaskJob(Guid downloadTaskId, CancellationToken cancellationToken = default)
    {
        var jobKey = DownloadJob.GetJobKey(downloadTaskId);
        if (!await _scheduler.IsJobRunning(jobKey, cancellationToken))
            return;

        var timeoutAt = DateTime.UtcNow.AddSeconds(30);
        while (await _scheduler.IsJobRunning(jobKey, cancellationToken) && DateTime.UtcNow < timeoutAt)
            await Task.Delay(100, cancellationToken);
    }

    public Task<bool> IsDownloading(DownloadTaskKey downloadTaskKey, CancellationToken cancellationToken = default)
    {
        var jobKey = DownloadJob.GetJobKey(downloadTaskKey.Id);
        return _scheduler.IsJobRunning(jobKey, cancellationToken);
    }

    public async Task<List<DownloadTaskKey>> GetCurrentlyDownloadingKeysByServer(int plexServerId)
    {
        var keys = await _scheduler.GetJobKeys(JobTypes.DownloadJob);
        var requests = await Task.WhenAll(keys.Select(x => _scheduler.GetJobDetail(x)));
        return requests
            .Select(x => x?.JobDataMap.GetPayload<DownloadJobPayload>()?.DownloadTaskKey)
            .OfType<DownloadTaskKey>()
            .Where(x => x.PlexServerId == plexServerId)
            .ToList();
    }

    public async Task<bool> IsServerDownloading(int plexServerId)
    {
        return (await GetCurrentlyDownloadingKeysByServer(plexServerId)).Any(x => x.PlexServerId == plexServerId);
    }
}
