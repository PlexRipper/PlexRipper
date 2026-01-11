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
            while (!linkedCts.Token.IsCancellationRequested)
            {
                // Give it some time to start, keep 500ms between checks
                await Task.Delay(500, linkedCts.Token);
                var jobs = await scheduler.GetCurrentlyExecutingJobs(linkedCts.Token);
                if (!jobs.Any(x => Equals(x.JobDetail.Key, key)))
                    break;

                await Task.Delay(500, linkedCts.Token);
            }
        }
        catch (OperationCanceledException)
            when (timeoutCts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            // Timeout occurred, but we'll exit gracefully without throwing
            // The job may still be running, but we don't want to block indefinitely
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
