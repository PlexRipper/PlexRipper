using Quartz;

namespace PlexRipper.Application;

public static class ISchedulerExtensions
{
    public static Task<bool> IsJobRunningAsync(this IScheduler scheduler, JobKey key, CancellationToken cancellationToken = default)
    {
        return scheduler.CheckExists(key, cancellationToken);
    }

    public static async Task<Result> ScheduleJobAsync(
        this IScheduler scheduler,
        IJobDetail jobDetail,
        ITrigger trigger,
        CancellationToken cancellationToken = default)
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
}