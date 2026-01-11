using Quartz;

namespace Reaparr.Application;

public static class ISchedulerExtensions
{
    public static Task<bool> IsJobRunningAsync(
        this IScheduler scheduler,
        JobKey key,
        CancellationToken cancellationToken = default
    ) => scheduler.CheckExists(key, cancellationToken);

    public static async Task<Result> ScheduleJobAsync(
        this IScheduler scheduler,
        IJobDetail jobDetail,
        ITrigger trigger,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            await scheduler.ScheduleJob(jobDetail, trigger, cancellationToken);
            return Result.Ok();
        }
        catch (Exception e)
        {
            return Result.Fail(new ExceptionalError(e)).LogError();
        }
    }

    /// <summary>
    /// Waits for a job to complete execution with a configurable timeout.
    /// </summary>
    /// <param name="scheduler">The Quartz scheduler instance.</param>
    /// <param name="key">The job key to monitor.</param>
    /// <param name="cancellationToken">External cancellation token.</param>
    /// <param name="timeoutSeconds">Maximum seconds to wait before returning (default 30).</param>
    /// <remarks>
    /// If the timeout expires, the method returns gracefully without throwing.
    /// The job may continue running in the background after timeout.
    /// </remarks>
    public static async Task AwaitJobRunning(
        this IScheduler scheduler,
        JobKey key,
        CancellationToken cancellationToken = default,
        int timeoutSeconds = 30
    )
    {
        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

        try
        {
            const int pollIntervalMs = 200;

            while (!linkedCts.Token.IsCancellationRequested)
            {
                var executingJobs = await scheduler.GetCurrentlyExecutingJobs(linkedCts.Token);
                var isJobStillRunning = executingJobs.Any(x => Equals(x.JobDetail.Key, key));

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

    public static Task<bool> StopJob(this IScheduler scheduler, JobKey key) => scheduler.Interrupt(key);

    public static Task<bool> IsJobRunning(this IScheduler scheduler, JobKey key) => scheduler.CheckExists(key);

    public static async Task<List<JobDataMap>> GetRunningJobDataMaps(this IScheduler scheduler, Type jobType)
    {
        var jobs = await scheduler.GetCurrentlyExecutingJobs();
        return jobs.Where(x => x.JobInstance.GetType() == jobType).Select(x => x.JobDetail.JobDataMap).ToList();
    }
}
