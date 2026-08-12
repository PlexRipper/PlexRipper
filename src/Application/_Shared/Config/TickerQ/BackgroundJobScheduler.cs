using TickerQ.Utilities;
using TickerQ.Utilities.Enums;
using TickerQ.Utilities.Interfaces;
using TickerQ.Utilities.Interfaces.Managers;
using TickerQ.Utilities.Models;

namespace Reaparr.Application;


/// <summary>
/// Implements Reaparr background-job lifecycle and persistence operations using TickerQ.
/// </summary>
public class BackgroundJobScheduler : IBackgroundJobScheduler
{
    private readonly ILogger _log;
    private readonly IReaparrDbContextFactory _dbContextFactory;
    private readonly ITimeTickerManager<JobTimeTicker> _tickerManager;
    private readonly ITickerQHostScheduler _hostScheduler;
    private readonly IAppRuntimeInfo _appRuntimeInfo;
    private readonly ICommandExecutor _commandExecutor;

    private static TickerStatus[] ActiveStatuses => [TickerStatus.Idle, TickerStatus.Queued, TickerStatus.InProgress];

    /// <summary>
    /// Initializes a scheduler that controls the TickerQ host, persists and queries tickers, and performs application
    /// recovery work when the scheduler starts.
    /// </summary>
    /// <param name="log">The application logger used for scheduler lifecycle diagnostics.</param>
    /// <param name="dbContextFactory">The factory used to query persisted ticker and occurrence state.</param>
    /// <param name="tickerManager">The TickerQ manager used to create one-time tickers.</param>
    /// <param name="hostScheduler">The TickerQ host lifecycle controller.</param>
    /// <param name="appRuntimeInfo">Runtime-mode information used to suppress production recovery work in integration tests.</param>
    /// <param name="commandExecutor">The command dispatcher used to clean up and restore library synchronization jobs.</param>
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

    /// <summary>
    /// Starts the TickerQ host when necessary and restores the library synchronization queue. Recovery commands are
    /// intentionally skipped in integration-test mode so tests can control their own persisted scheduler state.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels host startup or library synchronization recovery.</param>
    /// <returns>A successful result when the host is running and recovery succeeds; otherwise, a failed or canceled result.</returns>
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

    /// <summary>
    /// Removes stale library synchronization queue entries and enqueues any libraries that still require synchronization.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels either recovery command.</param>
    /// <returns>The first canceled or failed command result, or the successful queue-check result.</returns>
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

    /// <summary>
    /// Stops the TickerQ host if it is running and verifies that the host transitioned to the stopped state.
    /// Calling this method while the host is already stopped succeeds without performing additional work.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels the host shutdown operation.</param>
    /// <returns>A successful result when the host is stopped; otherwise, a failed result.</returns>
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

    /// <inheritdoc />
    public async Task<bool> Interrupt(JobKey jobKey, CancellationToken cancellationToken = default)
    {
        using var dbContext = await _dbContextFactory.CreateAsync();

        var timeTickerIds = await dbContext
            .TimeTickers.Where(x => x.JobKey == jobKey.Name && x.JobType == jobKey.Type)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        if (timeTickerIds.Exists(TickerCancellationTokenManager.RequestTickerCancellationById))
            return true;

        var cronOccurrenceIds = await dbContext
            .CronTickerOccurrences.Where(x =>
                x.CronTicker.JobKey == jobKey.Name && x.CronTicker.JobType == jobKey.Type
            )
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        return cronOccurrenceIds.Exists(TickerCancellationTokenManager.RequestTickerCancellationById);
    }

    /// <inheritdoc />
    public async Task<bool> IsJobRunning(
        JobKey jobKey,
        CancellationToken cancellationToken = default
    )
    {
        using var dbContext = await _dbContextFactory.CreateAsync();

        if (
            await dbContext.TimeTickers.AnyAsync(
                x =>
                    x.JobKey == jobKey.Name
                    && x.JobType == jobKey.Type
                    && Enumerable.Contains(ActiveStatuses, x.Status),
                cancellationToken
            )
        )
            return true;

        return await dbContext.CronTickerOccurrences.AnyAsync(
            x =>
                x.CronTicker.JobKey == jobKey.Name
                && x.CronTicker.JobType == jobKey.Type
                && Enumerable.Contains(ActiveStatuses, x.Status),
            cancellationToken
        );
    }

    /// <inheritdoc />
    public async Task<bool> CheckExists(JobKey jobKey)
    {
        using var dbContext = await _dbContextFactory.CreateAsync();

        if (
            await dbContext.TimeTickers.AnyAsync(x =>
                x.JobKey == jobKey.Name
                && x.JobType == jobKey.Type
                && Enumerable.Contains(ActiveStatuses, x.Status)
            )
        )
            return true;

        return await dbContext.CronTickers.AnyAsync(x => x.JobKey == jobKey.Name && x.JobType == jobKey.Type
        );
    }

    /// <inheritdoc />
    public Task<TickerResult<JobTimeTicker>> ExecuteJob<TFunction, TRequest>(
        JobKey jobKey,
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

    /// <inheritdoc />
    public Task<TickerResult<JobTimeTicker>> ScheduleJob<TFunction, TRequest>(
        JobKey jobKey,
        TRequest request,
        DateTime? executionTime = null,
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
            ExecutionTime = executionTime ?? DateTime.UtcNow,
        };
        return _tickerManager.AddAsync(ticker, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AwaitScheduler(CancellationToken cancellationToken = default)
    {
        while ((await GetCurrentlyExecutingJobs(cancellationToken)).Count > 0)
            await Task.Delay(250, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<JobStatusUpdate<string>>> GetCurrentlyExecutingJobs(
        CancellationToken cancellationToken = default
    )
    {
        using var dbContext = await _dbContextFactory.CreateAsync();

        var timeTickers = await dbContext
            .TimeTickers.Where(x => Enumerable.Contains(ActiveStatuses, x.Status))
            .Select(x => new { x.JobType, x.Request, x.Id, x.CreatedAt })
            .ToListAsync(cancellationToken);

        var cronTickers = await dbContext
            .CronTickerOccurrences.Where(x => Enumerable.Contains(ActiveStatuses, x.Status))
            .Select(x => new { x.CronTicker.JobType, x.CronTicker.Request, x.Id, x.CreatedAt })
            .ToListAsync(cancellationToken);

        return timeTickers
            .Select(x => ToJobStatusUpdate(x.JobType, x.Request, x.Id, x.CreatedAt))
            .Concat(cronTickers.Select(x => ToJobStatusUpdate(x.JobType, x.Request, x.Id, x.CreatedAt)))
            .ToList();
    }

    /// <summary>
    /// Converts a persisted TickerQ record projection into the application-level background-job status contract.
    /// </summary>
    /// <param name="jobType">The Reaparr job category persisted with the ticker.</param>
    /// <param name="request">The serialized request payload, when the ticker has one.</param>
    /// <param name="id">The unique TickerQ ticker or occurrence identifier.</param>
    /// <param name="createdAt">The UTC time at which TickerQ created the persisted record.</param>
    /// <returns>A status update containing the normalized job identifier and serialized request.</returns>
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