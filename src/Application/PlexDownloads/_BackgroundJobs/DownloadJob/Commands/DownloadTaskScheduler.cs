using TickerQ.Utilities.Enums;

namespace Reaparr.Application;

public class DownloadTaskScheduler : IDownloadTaskScheduler
{
    private readonly ILogger _log;
    private readonly IBackgroundJobScheduler _scheduler;
    private readonly IReaparrDbContextFactory _dbContextFactory;

    public DownloadTaskScheduler(
        ILogger log,
        IBackgroundJobScheduler scheduler,
        IReaparrDbContextFactory dbContextFactory
    )
    {
        _log = log.ForContext<DownloadTaskScheduler>();
        _scheduler = scheduler;
        _dbContextFactory = dbContextFactory;
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
            if (
                await _scheduler.IsJobRunning(jobKey, cancellationToken)
                || await _scheduler.IsQueued(jobKey)
            )
            {
                _log.Here().Debug("{DownloadJobName} with {JobKey} is already scheduled", nameof(DownloadJob), jobKey);
                return Result.Ok();
            }

            var schedulingResult = await _scheduler.ExecuteJob<DownloadJob, DownloadTaskKey>(
                jobKey,
                downloadTaskKey,
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
                if (!await _scheduler.IsJobRunning(jobKey, cancellationToken))
                {
                    return Result
                        .Fail($"{nameof(DownloadJob)} with {jobKey} cannot be stopped because it is not running")
                        .LogWarning();
                }

                var stopResult = await _scheduler.Interrupt(jobKey, cancellationToken);
                if (!stopResult)
                    return Result.Fail($"Failed to stop {nameof(DownloadTaskGeneric)} with id {downloadTaskKey}")
                        .LogError();

                if (waitForCompletion)
                {
                    await AwaitDownloadTaskJob(downloadTaskKey.Id, cancellationToken);
                }

                return Result.Ok();
            }
        );
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

    public async Task<List<DownloadTaskKey>> GetCurrentlyDownloadingKeysByServer(
        int plexServerId
    )
    {
        using var dbContext = await _dbContextFactory.CreateAsync();
        var requests = await dbContext.TimeTickers
            .Where(x =>
                x.JobType == JobTypes.DownloadJob
                && (x.Status == TickerStatus.Idle
                    || x.Status == TickerStatus.Queued
                    || x.Status == TickerStatus.InProgress)
            )
            .Select(x => x.Request)
            .ToListAsync();

        return requests
            .Select(x => JsonSerializer.Deserialize<DownloadTaskKey>(x))
            .OfType<DownloadTaskKey>()
            .Where(x => x.PlexServerId == plexServerId)
            .ToList();
    }

    public async Task<bool> IsServerDownloading(
        int plexServerId
    )
    {
        return (await GetCurrentlyDownloadingKeysByServer(plexServerId)).Any(x =>
            x.PlexServerId == plexServerId
        );
    }
}
