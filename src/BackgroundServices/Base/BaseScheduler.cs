using Quartz;

namespace BackgroundServices.Base;

public abstract class BaseScheduler
{
    private readonly IScheduler _scheduler;

    protected abstract JobKey DefaultJobKey { get; }


    protected JobKey GetJobKey(int id)
    {
        return new JobKey($"{DefaultJobKey.Name}_{id}", DefaultJobKey.Group);
    }

    protected async Task<bool> IsJobRunning(JobKey key)
    {
        var jobs = await _scheduler.GetCurrentlyExecutingJobs();
        if (!jobs.Any())
            return false;

        return jobs.FirstOrDefault(executionContext => Equals(executionContext.JobDetail.Key, key)) != null;
    }


    protected BaseScheduler(IScheduler scheduler)
    {
        _scheduler = scheduler;
    }

    protected async Task<Result> ScheduleJob(IJobDetail jobDetail, ITrigger trigger, CancellationToken cancellationToken = default)
    {
        try
        {
            await _scheduler.ScheduleJob(jobDetail, trigger, cancellationToken);
            return Result.Ok();
        }
        catch (Exception e)
        {
            return Result.Fail(new ExceptionalError(e)).LogError();
        }
    }
}