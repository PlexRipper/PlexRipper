using TickerQ.Utilities.Base;
using TickerQ.Utilities.Interfaces;

namespace Reaparr.Application;

/// <summary>
/// Base class for TickerQ jobs that publishes frontend lifecycle updates around
/// every execution. The wrapper applies equally to time tickers and cron ticker
/// occurrences because both are represented by <see cref="TickerFunctionContext{TRequest}"/>.
/// </summary>
public abstract class BaseBackgroundJob<TPayload, TUpdate> : ITickerFunction<TPayload>
    where TPayload : class
    where TUpdate : class
{
    private readonly ILogger _log;
    private readonly IProgressHubService _progressHubService;
    private readonly INotificationHubService _notificationHubService;

    protected BaseBackgroundJob(
        ILogger log,
        IProgressHubService progressHubService,
        INotificationHubService notificationHubService
    )
    {
        _log = log.ForContext(GetType());
        _progressHubService = progressHubService;
        _notificationHubService = notificationHubService;
    }

    protected abstract JobTypes JobType { get; }

    protected virtual List<RefreshDataType> RefreshDataTypes => [];

    public async Task ExecuteAsync(
        TickerFunctionContext<TPayload> context,
        CancellationToken cancellationToken = default
    )
    {
        var jobStartTime = DateTime.UtcNow;

        await PublishStatusUpdate(context, JobStatus.Started, jobStartTime);

        try
        {
            await ExecuteJobAsync(context, cancellationToken);
            await PublishStatusUpdate(context, JobStatus.Completed, jobStartTime);

            // Follow-up work runs after clients receive completion and must not
            // change an already completed primary job to failed.
            try
            {
                await ExecuteAfterCompletionAsync(context, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                _log.Here()
                    .Warning(
                        "Post-completion work was cancelled for {JobType} ticker {TickerId}",
                        JobType,
                        context.Id
                    );
            }
            catch (Exception e)
            {
                _log.Here()
                    .Warning(
                        e,
                        "Post-completion work failed for {JobType} ticker {TickerId}",
                        JobType,
                        context.Id
                    );
            }
        }
        catch (OperationCanceledException)
        {
            await PublishStatusUpdate(context, JobStatus.Cancelled, jobStartTime);
            throw; // Needs to throw for ThinkerQ to mark as Cancelled
        }
        catch
        {
            await PublishStatusUpdate(context, JobStatus.Failed, jobStartTime);
            throw; // Needs to throw for ThinkQ to mark as failed
        }
    }

    protected abstract Task ExecuteJobAsync(
        TickerFunctionContext<TPayload> context,
        CancellationToken cancellationToken
    );

    protected virtual Task ExecuteAfterCompletionAsync(
        TickerFunctionContext<TPayload> context,
        CancellationToken cancellationToken
    ) => Task.CompletedTask;

    protected virtual async Task<TUpdate?> GetStatusUpdateDataAsync(
        TickerFunctionContext<TPayload> context,
        CancellationToken cancellationToken
    ) => null;

    private async Task PublishStatusUpdate(
        TickerFunctionContext<TPayload> context,
        JobStatus status,
        DateTime jobStartTime
    )
    {
        try
        {
            var data = await GetStatusUpdateDataAsync(context, CancellationToken.None);
            if (data is null)
            {
                _log.Here()
                    .Warning(
                        "Could not create {JobType} frontend status update for ticker {TickerId}",
                        JobType,
                        context.Id
                    );
                return;
            }

            await _progressHubService.SendJobStatusUpdateAsync(
                new JobStatusUpdate<TUpdate>(
                    JobType,
                    status,
                    data,
                    context.Id.ToString(),
                    jobStartTime
                )
            );

            if (RefreshDataTypes.Count > 0)
                await _notificationHubService.SendRefreshNotificationAsync(RefreshDataTypes);
        }
        catch (Exception e)
        {
            // A listener failure must never fail or strand the actual background job.
            _log.Here()
                .Error(
                    e,
                    "Failed to publish {Status} update for {JobType} ticker {TickerId}",
                    status,
                    JobType,
                    context.Id
                );
        }
    }
}