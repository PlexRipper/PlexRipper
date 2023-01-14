using Quartz;

namespace PlexRipper.Application.BackgroundServices;

public abstract class BaseScheduler
{
    private readonly IScheduler _scheduler;

    protected abstract JobKey DefaultJobKey { get; }


    protected JobKey GetJobKey(int id)
    {
        return new JobKey($"{DefaultJobKey.Name}_{id}", DefaultJobKey.Group);
    }

    protected Task<bool> IsJobRunning(JobKey key)
    {
        return _scheduler.CheckExists(key);
    }

    protected async Task<bool> DeleteJob(JobKey key)
    {
        return await _scheduler.DeleteJob(key);
    }

    protected async Task TriggerJob(JobKey key, JobDataMap jobDataMap)
    {
        await _scheduler.TriggerJob(key, jobDataMap);
    }

    protected async Task<bool> StopJob(JobKey key)
    {
        return await _scheduler.Interrupt(key);
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