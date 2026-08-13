using TickerQ.Utilities.Enums;

namespace Reaparr.Application;

public class MoveDownloadFileJobScheduler : IMoveDownloadFileScheduler
{
    private readonly ILogger _log;
    private readonly IBackgroundJobScheduler _scheduler;
    private readonly IReaparrDbContextFactory _dbContextFactory;

    public MoveDownloadFileJobScheduler(
        ILogger log,
        IBackgroundJobScheduler scheduler,
        IReaparrDbContextFactory dbContextFactory
    )
    {
        _log = log.ForContext<MoveDownloadFileJobScheduler>();
        _scheduler = scheduler;
        _dbContextFactory = dbContextFactory;
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

        var schedulingResult = await _scheduler.ExecuteJob<MoveDownloadFileJob, DownloadTaskKey>(
            jobKey,
            downloadTaskKey,
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

    public Task<bool> IsDownloadFileMoving(
        DownloadTaskKey downloadTaskKey,
        CancellationToken cancellationToken
    ) => _scheduler.IsJobRunning(MoveDownloadFileJob.GetJobKey(downloadTaskKey.Id), cancellationToken);

    public async Task<bool> IsAnyMoveDownloadFileJobRunning()
    {
        using var dbContext = await _dbContextFactory.CreateAsync();
        return await dbContext.TimeTickers.AnyAsync(x =>
            x.JobType == JobTypes.MoveDownloadFileJob
            && (x.Status == TickerStatus.Idle
                || x.Status == TickerStatus.Queued
                || x.Status == TickerStatus.InProgress)
        );
    }

    public async Task<List<DownloadTaskKey>> GetCurrentlyMovingKeysByServer(int plexServerId)
    {
        using var dbContext = await _dbContextFactory.CreateAsync();
        var requests = await dbContext.TimeTickers
            .Where(x =>
                x.JobType == JobTypes.MoveDownloadFileJob
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
}
