using Quartz;

namespace PlexRipper.Application;

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
        CancellationToken cancellationToken = default
    )
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(500, cancellationToken);
            var jobs = await scheduler.GetCurrentlyExecutingJobs(cancellationToken);
            if (!jobs.Any(x => Equals(x.JobDetail.Key, key)))
                break;

            await Task.Delay(500, cancellationToken);
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
