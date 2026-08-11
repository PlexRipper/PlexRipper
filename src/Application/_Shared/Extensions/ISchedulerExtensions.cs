namespace Reaparr.Application;

public static class ISchedulerExtensions
{
    public static Task<bool> IsJobRunningAsync(
        this IScheduler scheduler,
        JobKey keyV2,
        CancellationToken cancellationToken = default
    ) => scheduler.CheckExists(keyV2, cancellationToken);

    public static async Task<Result> ScheduleJobAsync(
        this IScheduler scheduler,
        IJobDetail jobDetail,
        ITrigger trigger,
        CancellationToken cancellationToken = default
    )
    {
        return await Result.Try(async Task () => await scheduler.ScheduleJob(jobDetail, trigger, cancellationToken));
    }

    /// <summary>
    /// Waits for a job to complete execution with a configurable timeout.
    /// </summary>
    /// <param name="scheduler">The Quartz scheduler instance.</param>
    /// <param name="keyV2">The job key to monitor.</param>
    /// <param name="cancellationToken">External cancellation token.</param>
    /// <param name="timeoutSeconds">Maximum seconds to wait before returning (default 30).</param>
    /// <remarks>
    /// If the timeout expires, the method returns gracefully without throwing.
    /// The job may continue running in the background after timeout.
    /// </remarks>
    public static async Task AwaitJobCompletion(
        this IScheduler scheduler,
        JobKey keyV2,
        CancellationToken cancellationToken = default,
        int timeoutSeconds = 30
    )
    {
        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

        try
        {
            const int pollIntervalMs = 200;

            while (true)
            {
                if (linkedCts.IsCancellationRequested)
                    return;

                var executingJobs = await scheduler.GetCurrentlyExecutingJobs(linkedCts.Token);
                var isJobStillRunning = executingJobs.Any(x => Equals(x.JobDetail.Key, keyV2));

                if (!isJobStillRunning)
                    return;

                await Task.Delay(pollIntervalMs, linkedCts.Token);
            }
        }
        catch (OperationCanceledException)
            when (timeoutCts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            // Timeout expired - exit gracefully without throwing
            // The job may still be running in the background
        }
    }

    public static Task<bool> StopJob(
        this IScheduler scheduler,
        JobKey keyV2,
        CancellationToken cancellationToken = default
    ) => scheduler.Interrupt(keyV2, cancellationToken);

    public static Task<bool> IsJobRunning(
        this IScheduler scheduler,
        JobKey keyV2,
        CancellationToken cancellationToken = default
    ) => scheduler.CheckExists(keyV2, cancellationToken);

    public static async Task<List<JobDataMap>> GetRunningJobDataMaps(
        this IScheduler scheduler,
        Type jobType
    )
    {
        var jobs = await scheduler.GetCurrentlyExecutingJobs(CancellationToken.None);
        return jobs.Where(x => x.JobInstance.GetType() == jobType).Select(x => x.JobDetail.JobDataMap).ToList();
    }
}