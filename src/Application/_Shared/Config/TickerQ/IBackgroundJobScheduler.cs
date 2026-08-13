using TickerQ.Utilities.Interfaces;
using TickerQ.Utilities.Models;

namespace Reaparr.Application;

/// <summary>
/// Provides the application-facing abstraction for controlling the TickerQ host and managing Reaparr background jobs.
/// Implementations translate strongly typed job functions and requests into persisted TickerQ tickers, expose job-state
/// queries using Reaparr job keys, and coordinate graceful application startup and shutdown.
/// </summary>
public interface IBackgroundJobScheduler : ISetupAsync, IStopAsync
{
    /// <summary>
    /// Creates a one-time ticker for immediate execution of the specified background-job function.
    /// The supplied request is serialized into the ticker and the logical job key is persisted so that the job can
    /// subsequently be located, inspected, or interrupted through this scheduler.
    /// </summary>
    /// <typeparam name="TFunction">The TickerQ function that handles <typeparamref name="TRequest"/>.</typeparam>
    /// <typeparam name="TRequest">The request payload type accepted by the function.</typeparam>
    /// <param name="jobKey">The stable logical name and Reaparr job type used to identify the job.</param>
    /// <param name="request">The payload to serialize and pass to the background-job function.</param>
    /// <param name="cancellationToken">A token that cancels creation of the ticker; it does not interrupt the job after it has started.</param>
    /// <returns>A successful result when the ticker was created; otherwise, a failed result.</returns>
    Task<Result<JobTimeTicker>> ExecuteJob<TFunction, TRequest>(
        JobKey jobKey,
        TRequest request,
        CancellationToken cancellationToken = default
    )
        where TFunction : class, ITickerFunction<TRequest>;

    /// <summary>
    /// Creates a one-time ticker for execution at a requested UTC time. When no execution time is supplied, the ticker
    /// is scheduled for the current UTC time. The request and logical job key are persisted with the ticker.
    /// </summary>
    /// <typeparam name="TFunction">The TickerQ function that handles <typeparamref name="TRequest"/>.</typeparam>
    /// <typeparam name="TRequest">The request payload type accepted by the function.</typeparam>
    /// <param name="jobKey">The stable logical name and Reaparr job type used to identify the job.</param>
    /// <param name="request">The payload to serialize and pass to the background-job function.</param>
    /// <param name="executionTime">The UTC time at which the job should become eligible to run, or <see langword="null"/> to use the current UTC time.</param>
    /// <param name="cancellationToken">A token that cancels creation of the ticker; it does not interrupt the job after it has started.</param>
    /// <returns>A successful result when the ticker was created; otherwise, a failed result.</returns>

    // ReSharper disable once UnusedMember.Global -- Part of the scheduler abstraction for delayed jobs.
    Task<Result<JobTimeTicker>> ScheduleJob<TFunction, TRequest>(
        JobKey jobKey,
        TRequest request,
        DateTime? executionTime = null,
        CancellationToken cancellationToken = default
    )
        where TFunction : class, ITickerFunction<TRequest>;

    /// <summary>
    /// Creates multiple one-time tickers in one persistence operation.
    /// </summary>
    Task<Result<List<JobTimeTicker>>> ScheduleJobs<TFunction, TRequest>(
        IReadOnlyCollection<(JobKey JobKey, TRequest Request)> jobs,
        DateTime? executionTime = null,
        CancellationToken cancellationToken = default
    )
        where TFunction : class, ITickerFunction<TRequest>;

    /// <summary>
    /// Requests cancellation of a time ticker or cron occurrence matching the supplied logical job key.
    /// Matching persisted records are examined until TickerQ accepts a cancellation request for one occurrence.
    /// </summary>
    /// <param name="jobKey">The logical job name and type whose current occurrence should be interrupted.</param>
    /// <param name="cancellationToken">A token that cancels the database lookup used to find matching ticker identifiers.</param>
    /// <returns><see langword="true"/> when TickerQ accepted a cancellation request; otherwise, <see langword="false"/>.</returns>
    Task<bool> Interrupt(JobKey jobKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes queued one-time tickers and requests cooperative cancellation for running tickers matching the exact
    /// supplied job keys. Completed history is retained.
    /// </summary>
    /// <param name="jobKeys">The exact logical job names and types to invalidate.</param>
    /// <param name="cancellationToken">A token that cancels database lookup and queued-ticker deletion.</param>
    /// <returns>A successful result when all queued deletions were accepted.</returns>
    Task<Result> DeleteBatchJobs(
        IReadOnlyCollection<JobKey> jobKeys,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Determines whether a one-time ticker or cron occurrence with the supplied logical key currently has an active
    /// status. Idle, queued, and in-progress records are considered running for application coordination purposes.
    /// </summary>
    /// <param name="jobKey">The logical job name and type to inspect.</param>
    /// <param name="cancellationToken">A token that cancels the database query.</param>
    /// <returns><see langword="true"/> when an active matching ticker or occurrence exists; otherwise, <see langword="false"/>.</returns>
    Task<bool> IsJobRunning(JobKey jobKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether the scheduler contains an active one-time ticker or a registered cron ticker with the
    /// supplied logical key. Unlike <see cref="IsJobRunning"/>, a cron definition counts as existing even when none of
    /// its occurrences is currently executing.
    /// </summary>
    /// <param name="jobKey">The logical job name and type to locate.</param>
    /// <returns><see langword="true"/> when a matching active time ticker or cron definition exists; otherwise, <see langword="false"/>.</returns>
    Task<bool> IsQueued(JobKey jobKey);

    /// <summary>
    /// Retrieves application-level status records for all one-time tickers and cron occurrences that are idle, queued,
    /// or in progress. Persisted requests are retained in serialized form in the returned status models.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels the database queries.</param>
    /// <returns>A list containing the currently active jobs from both TickerQ scheduling mechanisms.</returns>
    Task<List<JobStatusUpdate<string>>> GetCurrentlyExecutingJobs(CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously waits until no jobs reported by <see cref="GetCurrentlyExecutingJobs"/> remain active. The
    /// scheduler is polled at a short interval, making this suitable for graceful shutdown and test synchronization.
    /// </summary>
    /// <param name="cancellationToken">A token that stops waiting and cancels both polling queries and delays.</param>
    /// <returns>A task that completes when the active-job collection is empty.</returns>
    Task AwaitScheduler(CancellationToken cancellationToken = default);
}