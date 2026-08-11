using TickerQ.Utilities;
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

    public async Task<bool> CheckExists(JobKeyV2 jobKey)
    {
        using var dbContext = await _contextFactory.CreateAsync();

        var timeTickerExists = await dbContext.TimeTickers.AnyAsync(
            x => x.JobKey == jobKey.Name && x.JobType == jobKey.Type,
            CancellationToken.None
        );

        if (timeTickerExists)
            return true;

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
