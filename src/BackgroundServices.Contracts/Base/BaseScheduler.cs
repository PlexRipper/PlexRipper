using FluentResults;
using Logging.Interface;
using Quartz;

namespace BackgroundServices.Contracts;

public abstract class BaseScheduler
{
    #region Fields

    protected readonly ILog _log;

    private readonly IScheduler _scheduler;

    #endregion

    #region Constructors

    protected BaseScheduler(ILog log, IScheduler scheduler)
    {
        _log = log;
        _scheduler = scheduler;
    }

    #endregion

    #region Properties

    protected abstract JobKey DefaultJobKey { get; }

    #endregion

    protected JobKey GetJobKey(int id)
    {
        return new JobKey($"{DefaultJobKey.Name}_{id}", DefaultJobKey.Group);
    }

    protected Task<bool> IsJobRunning(JobKey key)
    {
        return _scheduler.CheckExists(key);
    }

    protected async Task AwaitJobRunning(JobKey key, CancellationToken cancellationToken = default)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var jobs = await _scheduler.GetCurrentlyExecutingJobs(cancellationToken);
            if (!jobs.Any(x => Equals(x.JobDetail.Key, key)))
                break;

            await Task.Delay(500, cancellationToken);
        }
    }

    protected async Task<List<JobDataMap>> GetRunningJobDataMaps(Type jobType)
    {
        var jobs = await _scheduler.GetCurrentlyExecutingJobs();
        return jobs
            .Where(x => x.JobInstance.GetType() == jobType)
            .Select(x => x.JobDetail.JobDataMap)
            .ToList();
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