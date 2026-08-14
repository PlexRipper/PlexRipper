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
    private readonly ICronTickerManager<JobCronTicker> _cronTickerManager;
    private readonly ITickerQHostScheduler _hostScheduler;
    private readonly IAppRuntimeInfo _appRuntimeInfo;
    private readonly ICommandExecutor _commandExecutor;

    private static IEnumerable<TickerStatus> QueuedStatuses => [TickerStatus.Idle, TickerStatus.Queued];

    /// <summary>
    /// Initializes a scheduler that controls the TickerQ host, persists and queries tickers, and performs application
    /// recovery work when the scheduler starts.
    /// </summary>
    /// <param name="log">The application logger used for scheduler lifecycle diagnostics.</param>
    /// <param name="dbContextFactory">The factory used to query persisted ticker and occurrence state.</param>
    /// <param name="tickerManager">The TickerQ manager used to create one-time tickers.</param>
    /// <param name="cronTickerManager">The TickerQ manager used to create recurring tickers.</param>
    /// <param name="hostScheduler">The TickerQ host lifecycle controller.</param>
    /// <param name="appRuntimeInfo">Runtime-mode information used to suppress production recovery work in integration tests.</param>
    /// <param name="commandExecutor">The command dispatcher used to clean up and restore library synchronization jobs.</param>
    public BackgroundJobScheduler(
        ILogger log,
        IReaparrDbContextFactory dbContextFactory,
        ITimeTickerManager<JobTimeTicker> tickerManager,
        ICronTickerManager<JobCronTicker> cronTickerManager,
        ITickerQHostScheduler hostScheduler,
        IAppRuntimeInfo appRuntimeInfo,
        ICommandExecutor commandExecutor
    )
    {
        _log = log.ForContext<BackgroundJobScheduler>();
        _dbContextFactory = dbContextFactory;
        _tickerManager = tickerManager;
        _cronTickerManager = cronTickerManager;
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

        var setupCronTickersResult = await SetupCronTickers(cancellationToken);
        if (setupCronTickersResult.IsCancelled || setupCronTickersResult.IsFailed)
            return setupCronTickersResult;

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

    private async Task<Result> SetupCronTickers(CancellationToken cancellationToken)
    {
        var cronTickers = new[]
        {
            CreateCronTicker<CheckPlexLibrariesForUpdatesJob>(
                CheckPlexLibrariesForUpdatesJob.GetJobKey(),
                "0 0 */3 * * *" // Every 3 hours
            ),
            CreateCronTicker<CheckForUpdateJob>(
                CheckForUpdateJob.GetJobKey(),
                "0 0 * * * *" // Every hour
            ),
            CreateCronTicker<CheckAllConnectionsStatusByPlexServerJob>(
                CheckAllConnectionsStatusByPlexServerJob.GetJobKey(),
                "0 */10 * * * *" // Every 10 minutes
            ),
            CreateCronTicker<RefreshPlexAccountAccessJob>(
                RefreshPlexAccountAccessJob.GetJobKey(),
                "0 0 */6 * * *" // Every 6 hours
            ),
        };

        using var dbContext = await _dbContextFactory.CreateAsync();
        var existingTickers = await dbContext
            .CronTickers
            .Where(x => cronTickers.Select(y => y.Function).Contains(x.Function))
            .ToListAsync(cancellationToken);

        foreach (var cronTicker in cronTickers)
        {
            var existingTicker = existingTickers.FirstOrDefault(x => x.Function == cronTicker.Function);
            if (
                existingTicker is not null
                && existingTicker.JobKey == cronTicker.JobKey
                && existingTicker.JobType == cronTicker.JobType
                && existingTicker.Expression == cronTicker.Expression
            )
                continue;

            TickerResult<JobCronTicker> tickerResult;
            if (existingTicker is null)
            {
                tickerResult = await _cronTickerManager.AddAsync(cronTicker, cancellationToken);
            }
            else
            {
                existingTicker.JobKey = cronTicker.JobKey;
                existingTicker.JobType = cronTicker.JobType;
                existingTicker.Expression = cronTicker.Expression;
                tickerResult = await _cronTickerManager.UpdateAsync(existingTicker, cancellationToken);
            }

            if (!tickerResult.IsSucceeded)
                return tickerResult.Exception is null
                    ? Result.Fail($"Failed to register recurring background job {cronTicker.JobKey}")
                    : Result.Fail(new ExceptionalError(tickerResult.Exception));
        }

        return Result.Ok();
    }

    private static JobCronTicker CreateCronTicker<TFunction>(JobKey jobKey, string expression) =>
        new()
        {
            Function = typeof(TFunction).Name,
            Expression = expression,
            IsEnabled = true,
            JobKey = jobKey.Name,
            JobType = jobKey.Type,
        };

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
    public async Task<Result> DeleteBatchJobs(
        IReadOnlyCollection<JobKey> jobKeys,
        CancellationToken cancellationToken = default
    )
    {
        if (jobKeys.Count == 0)
            return Result.Ok();

        using var dbContext = await _dbContextFactory.CreateAsync();
        var matchingTickers = new List<(Guid Id, TickerStatus Status)>();
        foreach (var group in jobKeys.Distinct().GroupBy(x => x.Type))
        {
            var names = group.Select(x => x.Name).ToList();
            var tickers = await dbContext
                .TimeTickers.Where(x =>
                    x.JobType == group.Key
                    && names.Contains(x.JobKey)
                    && (
                        x.Status == TickerStatus.Idle
                        || x.Status == TickerStatus.Queued
                        || x.Status == TickerStatus.InProgress
                    )
                )
                .Select(x => new ValueTuple<Guid, TickerStatus>(x.Id, x.Status))
                .ToListAsync(cancellationToken);
            matchingTickers.AddRange(tickers);
        }

        var queuedIds = matchingTickers
            .Where(x => x.Status is TickerStatus.Idle or TickerStatus.Queued)
            .Select(x => x.Id)
            .Distinct()
            .ToList();
        var runningIds = matchingTickers
            .Where(x => x.Status == TickerStatus.InProgress)
            .Select(x => x.Id)
            .Distinct()
            .ToList();

        var deletedCount = 0;
        if (queuedIds.Count > 0)
        {
            var deleteResult = await _tickerManager.DeleteBatchAsync(queuedIds, cancellationToken);
            if (!deleteResult.IsSucceeded)
                return deleteResult.Exception is null
                    ? Result.Fail("Failed to delete queued background jobs")
                    : Result.Fail(new ExceptionalError(deleteResult.Exception));

            deletedCount = queuedIds.Count;
        }

        var cancellationRequestedCount = runningIds.Count(TickerCancellationTokenManager.RequestTickerCancellationById);

        _log.Here()
            .Debug(
                "Invalidated background jobs: deleted {DeletedCount}, requested cancellation for {CancellationCount}",
                deletedCount,
                cancellationRequestedCount
            );

        return Result.Ok();
    }

    /// <inheritdoc />
    public async Task<bool> IsJobRunning(JobKey jobKey, CancellationToken cancellationToken = default)
    {
        using var dbContext = await _dbContextFactory.CreateAsync();

        if (
            await dbContext.TimeTickers.AnyAsync(
                x => x.JobKey == jobKey.Name && x.JobType == jobKey.Type && x.Status == TickerStatus.InProgress,
                cancellationToken
            )
        )
            return true;

        return await dbContext.CronTickerOccurrences.AnyAsync(
            x =>
                x.CronTicker.JobKey == jobKey.Name
                && x.CronTicker.JobType == jobKey.Type
                && x.Status == TickerStatus.InProgress,
            cancellationToken
        );
    }

    /// <inheritdoc />
    public async Task<bool> IsQueued(JobKey jobKey)
    {
        using var dbContext = await _dbContextFactory.CreateAsync();

        if (
            await dbContext.TimeTickers.AnyAsync(x =>
                x.JobKey == jobKey.Name && x.JobType == jobKey.Type && QueuedStatuses.Contains(x.Status)
            )
        )
            return true;

        return await dbContext.CronTickers.AnyAsync(x => x.JobKey == jobKey.Name && x.JobType == jobKey.Type);
    }

    /// <inheritdoc />
    public Task<Result<JobTimeTicker>> ExecuteJob<TFunction, TRequest>(
        JobKey jobKey,
        TRequest request,
        CancellationToken cancellationToken = default
    )
        where TFunction : class, ITickerFunction<TRequest>
    {
        if (!TryCreateTimeTicker<TFunction, TRequest>(jobKey, request, out var ticker, out var error))
            return Task.FromResult(Result.Fail<JobTimeTicker>(error));

        return AddTickerWithoutCallerExecutionContext(ticker, cancellationToken);
    }

    /// <inheritdoc />
    public Task<Result<JobTimeTicker>> ScheduleJob<TFunction, TRequest>(
        JobKey jobKey,
        TRequest request,
        DateTime? executionTime = null,
        CancellationToken cancellationToken = default
    )
        where TFunction : class, ITickerFunction<TRequest>
    {
        if (!TryCreateTimeTicker<TFunction, TRequest>(jobKey, request, out var ticker, out var error))
            return Task.FromResult(Result.Fail<JobTimeTicker>(error));

        ticker.ExecutionTime = executionTime ?? DateTime.UtcNow;
        return AddTickerWithoutCallerExecutionContext(ticker, cancellationToken);
    }

    /// <inheritdoc />
    public Task<Result<List<JobTimeTicker>>> ScheduleJobs<TFunction, TRequest>(
        IReadOnlyCollection<(JobKey JobKey, TRequest Request)> jobs,
        DateTime? executionTime = null,
        CancellationToken cancellationToken = default
    )
        where TFunction : class, ITickerFunction<TRequest>
    {
        var scheduledAt = executionTime ?? DateTime.UtcNow;
        var tickers = new List<JobTimeTicker>(jobs.Count);
        foreach (var (jobKey, request) in jobs)
        {
            if (!TryCreateTimeTicker<TFunction, TRequest>(jobKey, request, out var ticker, out var error))
                return Task.FromResult(Result.Fail<List<JobTimeTicker>>(error));

            ticker.ExecutionTime = scheduledAt;
            tickers.Add(ticker);
        }

        return AddTickersWithoutCallerExecutionContext(tickers, cancellationToken);
    }

    private static bool TryCreateTimeTicker<TFunction, TRequest>(
        JobKey jobKey,
        TRequest request,
        out JobTimeTicker ticker,
        out string error
    )
        where TFunction : class, ITickerFunction<TRequest>
    {
        ticker = null!;
        if (request is null)
        {
            error = $"Background job {jobKey.Name} cannot be queued without a request";
            return false;
        }

        var serializedRequest = TickerHelper.CreateTickerRequest(request);
        if (serializedRequest is not { Length: > 0 })
        {
            error = $"Background job {jobKey.Name} cannot be queued without a serialized request";
            return false;
        }

        ticker = new JobTimeTicker
        {
            Function = TickerFunctionProvider.GetFunctionName<TFunction>(),
            Request = serializedRequest,
            RequestJson = request switch
            {
                LibrarySyncJobPayload payload => new JobTimeTickerRequestProperties
                {
                    PlexLibraryId = payload.PlexLibraryId,
                    PlexServerId = payload.PlexServerId,
                },
                PlexLibraryComparisonJobPayload payload => new JobTimeTickerRequestProperties
                {
                    OwnedPlexLibraryId = payload.OwnedPlexLibraryId,
                    RemotePlexLibraryId = payload.RemotePlexLibraryId,
                },
                _ => null,
            },
            JobKey = jobKey.Name,
            JobType = jobKey.Type,
        };
        error = string.Empty;
        return true;
    }

    /// <summary>
    /// Hands a ticker to TickerQ without flowing request-scoped AsyncLocal state into its immediate-dispatch worker.
    /// TickerQ creates a dedicated service scope for each execution, but its immediate dispatch path queues work while
    /// still on the caller's execution context. Suppressing flow at this background-work boundary prevents an ambient
    /// HttpContext and other request state from outliving their request.
    /// </summary>
    private async Task<Result<JobTimeTicker>> AddTickerWithoutCallerExecutionContext(
        JobTimeTicker ticker,
        CancellationToken cancellationToken
    )
    {
        Task<TickerResult<JobTimeTicker>> addTask;
        if (ExecutionContext.IsFlowSuppressed())
            addTask = _tickerManager.AddAsync(ticker, cancellationToken);
        else
        {
            using (ExecutionContext.SuppressFlow())
                addTask = _tickerManager.AddAsync(ticker, cancellationToken);
        }

        var tickerResult = await addTask;
        return ToResult(tickerResult, $"Failed to schedule background job {ticker.JobKey}");
    }

    private async Task<Result<List<JobTimeTicker>>> AddTickersWithoutCallerExecutionContext(
        List<JobTimeTicker> tickers,
        CancellationToken cancellationToken
    )
    {
        Task<TickerResult<List<JobTimeTicker>>> addTask;
        if (ExecutionContext.IsFlowSuppressed())
            addTask = _tickerManager.AddBatchAsync(tickers, cancellationToken);
        else
        {
            using (ExecutionContext.SuppressFlow())
                addTask = _tickerManager.AddBatchAsync(tickers, cancellationToken);
        }

        var tickerResult = await addTask;
        return ToResult(tickerResult, "Failed to schedule background jobs");
    }

    private static Result<T> ToResult<T>(TickerResult<T> tickerResult, string errorMessage)
        where T : class
    {
        if (tickerResult.IsSucceeded)
            return Result.Ok(tickerResult.Result);

        return tickerResult.Exception is null
            ? Result.Fail<T>(errorMessage)
            : Result.Fail<T>(new ExceptionalError(tickerResult.Exception));
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
            .TimeTickers.Where(x => x.Status == TickerStatus.InProgress)
            .Select(x => new { x.JobType, x.Request, x.Id, x.CreatedAt })
            .ToListAsync(cancellationToken);

        var cronTickers = await dbContext
            .CronTickerOccurrences.Where(x => x.Status == TickerStatus.InProgress)
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