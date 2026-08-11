using TickerQ.Utilities;
using TickerQ.Utilities.Enums;
using TickerQ.Utilities.Interfaces;
using TickerQ.Utilities.Interfaces.Managers;
using TickerQ.Utilities.Models;

namespace Reaparr.Application;

public interface IBackgroundJobScheduler
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
}

public class BackgroundJobScheduler : IBackgroundJobScheduler
{
    private readonly IReaparrDbContextFactory _contextFactory;
    private readonly ITimeTickerManager<JobTimeTicker> _tickerManager;

    public BackgroundJobScheduler(
        IReaparrDbContextFactory contextFactory,
        ITimeTickerManager<JobTimeTicker> tickerManager
    )
    {
        _contextFactory = contextFactory;
        _tickerManager = tickerManager;
    }

    public async Task<bool> Interrupt(JobKeyV2 jobKey, CancellationToken cancellationToken = default)
    {
        using var dbContext = await _contextFactory.CreateAsync();

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
        using var dbContext = await _contextFactory.CreateAsync();

        var activeStatuses = new[]
        {
            TickerStatus.Idle,
            TickerStatus.Queued,
            TickerStatus.InProgress,
        };

        if (
            await dbContext.TimeTickers.AnyAsync(
                x =>
                    x.JobKey == jobKey.Name
                    && x.JobType == jobKey.Type
                    && activeStatuses.Contains(x.Status),
                cancellationToken
            )
        )
            return true;

        return await dbContext.CronTickerOccurrences.AnyAsync(
            x =>
                x.CronTicker.JobKey == jobKey.Name
                && x.CronTicker.JobType == jobKey.Type
                && activeStatuses.Contains(x.Status),
            cancellationToken
        );
    }

    public async Task<bool> CheckExists(JobKeyV2 jobKey)
    {
        using var dbContext = await _contextFactory.CreateAsync();

        // Time tickers are retained after execution as job history. Only an active
        // ticker should prevent another run with the same key from being scheduled.
        // Completed, failed, and cancelled tickers must not block a re-run.
        var activeTimeTickerExists = await dbContext.TimeTickers.AnyAsync(
            x => x.JobKey == jobKey.Name
                 && x.JobType == jobKey.Type
                 && (x.Status == TickerStatus.Idle
                     || x.Status == TickerStatus.Queued
                     || x.Status == TickerStatus.InProgress),
            CancellationToken.None
        );

        if (activeTimeTickerExists)
            return true;

        // A cron ticker is a persistent schedule rather than an execution record,
        // so its definition existing is sufficient here.
        return await dbContext.CronTickers.AnyAsync(
            x => x.JobKey == jobKey.Name && x.JobType == jobKey.Type,
            CancellationToken.None
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
        var ticker = new JobTimeTicker
        {
            Function = TickerFunctionProvider.GetFunctionName<TFunction>(),
            ExecutionTime = executionTime,
            Request = TickerHelper.CreateTickerRequest(request),
            JobKey = jobKey.Name,
            JobType = jobKey.Type,
        };

        return _tickerManager.AddAsync(ticker, cancellationToken);
    }
}
