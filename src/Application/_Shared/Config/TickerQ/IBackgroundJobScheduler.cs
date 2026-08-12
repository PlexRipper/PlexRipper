using TickerQ.Utilities;
using TickerQ.Utilities.Enums;
using TickerQ.Utilities.Interfaces;
using TickerQ.Utilities.Interfaces.Managers;
using TickerQ.Utilities.Models;

namespace Reaparr.Application;

public interface IBackgroundJobScheduler : ISetupAsync, IStopAsync
{
    Task<TickerResult<JobTimeTicker>> ExecuteJob<TFunction, TRequest>(
        JobKeyV2 jobKey,
        TRequest request,
        CancellationToken cancellationToken = default
    )
        where TFunction : class, ITickerFunction<TRequest>;

    Task<TickerResult<JobTimeTicker>> ScheduleJob<TFunction, TRequest>(
        JobKeyV2 jobKey,
        TRequest request,
        DateTime? executionTime = null,
        CancellationToken cancellationToken = default
    )
        where TFunction : class, ITickerFunction<TRequest>;

    Task<bool> Interrupt(JobKeyV2 jobKey, CancellationToken cancellationToken = default);

    Task<bool> IsJobRunning(JobKeyV2 jobKey, CancellationToken cancellationToken = default);

    Task<bool> CheckExists(JobKeyV2 jobKey);

    Task<List<JobStatusUpdate<string>>> GetCurrentlyExecutingJobs(CancellationToken cancellationToken = default);

    Task AwaitScheduler(CancellationToken cancellationToken = default);
}

public class BackgroundJobScheduler : IBackgroundJobScheduler
{
    private readonly ILogger _log;
    private readonly IReaparrDbContextFactory _dbContextFactory;
    private readonly ITimeTickerManager<JobTimeTicker> _tickerManager;
    private readonly ITickerQHostScheduler _hostScheduler;
    private readonly IAppRuntimeInfo _appRuntimeInfo;
    private readonly ICommandExecutor _commandExecutor;

    private static TickerStatus[] ActiveStatuses => [TickerStatus.Idle, TickerStatus.Queued, TickerStatus.InProgress];

    public BackgroundJobScheduler(
        ILogger log,
        IReaparrDbContextFactory dbContextFactory,
        ITimeTickerManager<JobTimeTicker> tickerManager,
        ITickerQHostScheduler hostScheduler,
        IAppRuntimeInfo appRuntimeInfo,
        ICommandExecutor commandExecutor
    )
    {
        _log = log.ForContext<BackgroundJobScheduler>();
        _dbContextFactory = dbContextFactory;
        _tickerManager = tickerManager;
        _hostScheduler = hostScheduler;
        _appRuntimeInfo = appRuntimeInfo;
        _commandExecutor = commandExecutor;
    }

    public async Task<Result> SetupAsync(CancellationToken cancellationToken = default)
    {
        if (!_hostScheduler.IsRunning)
        {
            _log.Here().Debug("Starting BackgroundJobScheduler scheduler");
            await _hostScheduler.StartAsync(cancellationToken);
        }

        if (!_appRuntimeInfo.IsIntegrationTestMode)
        {
            var setupLibrarySyncResult = await SetupLibrarySyncJobs(cancellationToken);
            if (setupLibrarySyncResult.IsCancelled || setupLibrarySyncResult.IsFailed)
                return setupLibrarySyncResult;
        }

        return _hostScheduler.IsRunning
            ? Result.Ok()
            : Result.Fail("Could not start BackgroundJobScheduler scheduler").LogError();
    }

    private async Task<Result> SetupLibrarySyncJobs(CancellationToken cancellationToken)
    {
        var cleanupResult = await _commandExecutor.Send(new CleanupLibrarySyncJobQueueCommand(), cancellationToken);
        if (cleanupResult.IsCancelled)
            return cleanupResult.LogWarning();

        if (cleanupResult.IsFailed)
            return cleanupResult.LogError();

        var checkQueuedResult = await _commandExecutor.Send(
            new CheckQueuedPlexLibraryToSyncCommand(),
            cancellationToken
        );
        return checkQueuedResult.IsFailed ? checkQueuedResult.LogError() : checkQueuedResult;
    }

    public async Task<Result> StopAsync(CancellationToken cancellationToken = default)
    {
        if (!_hostScheduler.IsRunning)
            return Result.Ok();

        _log.Here().Debug("Stopping BackgroundJobScheduler");
        await _hostScheduler.StopAsync(cancellationToken);

        return !_hostScheduler.IsRunning
            ? Result.Ok()
            : Result.Fail("Could not stop BackgroundJobScheduler").LogError();
    }

    public async Task<bool> Interrupt(JobKeyV2 jobKey, CancellationToken cancellationToken = default)
    {
        using var dbContext = await _dbContextFactory.CreateAsync();

        var timeTickerIds = await dbContext
            .TimeTickers.Where(x => x.JobKey == jobKey.Name && x.JobType == jobKey.Type)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        foreach (var tickerId in timeTickerIds)
        {
            if (TickerCancellationTokenManager.RequestTickerCancellationById(tickerId))
                return true;
        }

        var cronOccurrenceIds = await dbContext
            .CronTickerOccurrences.Where(x =>
                x.CronTicker.JobKey == jobKey.Name && x.CronTicker.JobType == jobKey.Type
            )
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        foreach (var occurrenceId in cronOccurrenceIds)
        {
            if (TickerCancellationTokenManager.RequestTickerCancellationById(occurrenceId))
                return true;
        }

        return false;
    }

    public async Task<bool> IsJobRunning(
        JobKeyV2 jobKey,
        CancellationToken cancellationToken = default
    )
    {
        using var dbContext = await _dbContextFactory.CreateAsync();

        if (
            await dbContext.TimeTickers.AnyAsync(
                x =>
                    x.JobKey == jobKey.Name
                    && x.JobType == jobKey.Type
                    && ActiveStatuses.Contains(x.Status),
                cancellationToken
            )
        )
            return true;

        return await dbContext.CronTickerOccurrences.AnyAsync(
            x =>
                x.CronTicker.JobKey == jobKey.Name
                && x.CronTicker.JobType == jobKey.Type
                && ActiveStatuses.Contains(x.Status),
            cancellationToken
        );
    }

    public async Task<bool> CheckExists(JobKeyV2 jobKey)
    {
        using var dbContext = await _dbContextFactory.CreateAsync();

        if (
            await dbContext.TimeTickers.AnyAsync(x =>
                x.JobKey == jobKey.Name
                && x.JobType == jobKey.Type
                && ActiveStatuses.Contains(x.Status)
            )
        )
            return true;

        return await dbContext.CronTickers.AnyAsync(x => x.JobKey == jobKey.Name && x.JobType == jobKey.Type
        );
    }

    public Task<TickerResult<JobTimeTicker>> ExecuteJob<TFunction, TRequest>(
        JobKeyV2 jobKey,
        TRequest request,
        CancellationToken cancellationToken = default
    )
        where TFunction : class, ITickerFunction<TRequest>
    {
        var ticker = new JobTimeTicker
        {
            Function = TickerFunctionProvider.GetFunctionName<TFunction>(),
            Request = TickerHelper.CreateTickerRequest(request),
            JobKey = jobKey.Name,
            JobType = jobKey.Type,
        };

        return _tickerManager.AddAsync(ticker, cancellationToken);
    }

    public Task<TickerResult<JobTimeTicker>> ScheduleJob<TFunction, TRequest>(
        JobKeyV2 jobKey,
        TRequest request,
        DateTime? executionTime = null,
        CancellationToken cancellationToken = default
    )
        where TFunction : class, ITickerFunction<TRequest>
    {
        var ticker = new JobTimeTicker()
        {
            Function = TickerFunctionProvider.GetFunctionName<TFunction>(),
            Request = TickerHelper.CreateTickerRequest(request),
            JobKey = jobKey.Name,
            JobType = jobKey.Type,
        };
        ticker.ExecutionTime = executionTime ?? DateTime.UtcNow;
        return _tickerManager.AddAsync(ticker, cancellationToken);
    }

    public async Task AwaitScheduler(CancellationToken cancellationToken = default)
    {
        while ((await GetCurrentlyExecutingJobs(cancellationToken)).Count > 0)
            await Task.Delay(250, cancellationToken);
    }

    public async Task<List<JobStatusUpdate<string>>> GetCurrentlyExecutingJobs(
        CancellationToken cancellationToken = default
    )
    {
        using var dbContext = await _dbContextFactory.CreateAsync();

        var timeTickers = await dbContext
            .TimeTickers.Where(x => ActiveStatuses.Contains(x.Status))
            .Select(x => new { x.JobType, x.Request, x.Id, x.CreatedAt })
            .ToListAsync(cancellationToken);

        var cronTickers = await dbContext
            .CronTickerOccurrences.Where(x => ActiveStatuses.Contains(x.Status))
            .Select(x => new { x.CronTicker.JobType, x.CronTicker.Request, x.Id, x.CreatedAt })
            .ToListAsync(cancellationToken);

        return timeTickers
            .Select(x => ToJobStatusUpdate(x.JobType, x.Request, x.Id, x.CreatedAt))
            .Concat(cronTickers.Select(x => ToJobStatusUpdate(x.JobType, x.Request, x.Id, x.CreatedAt)))
            .ToList();
    }

    private static JobStatusUpdate<string> ToJobStatusUpdate(
        JobTypes jobType,
        byte[]? request,
        Guid id,
        DateTime createdAt
    ) => new(
        jobType,
        JobStatus.Started,
        request is null ? string.Empty : System.Text.Encoding.UTF8.GetString(request),
        id.ToString(),
        createdAt
    );
}