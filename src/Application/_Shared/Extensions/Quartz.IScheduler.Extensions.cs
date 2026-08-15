using Quartz.Impl.Matchers;

namespace Reaparr.Application;

public static partial class QuartzExtensions
{
    public static Task<Result<DateTimeOffset>> ExecuteJob<TJob, TPayload>(
        this IScheduler scheduler,
        JobKey jobKey,
        TPayload payload,
        CancellationToken cancellationToken = default,
        DateTimeOffset? executeAt = null
    )
        where TJob : IJob => scheduler.ExecuteJob<TJob>(jobKey, payload.ToJobDataMap(), cancellationToken, executeAt);

    public static Task<Result> ExecuteJobs<TJob, TPayload>(
        this IScheduler scheduler,
        IReadOnlyCollection<(JobKey JobKey, TPayload Payload)> jobs,
        CancellationToken cancellationToken = default,
        DateTimeOffset? executeAt = null
    )
        where TJob : IJob =>
        scheduler.ExecuteJobs<TJob>(
            jobs.Select(x => (x.JobKey, x.Payload.ToJobDataMap())).ToList(),
            cancellationToken,
            executeAt
        );

    public static async Task<Result<DateTimeOffset>> ExecuteJob<TJob>(
        this IScheduler scheduler,
        JobKey jobKey,
        JobDataMap jobDataMap,
        CancellationToken cancellationToken = default,
        DateTimeOffset? executeAt = null
    )
        where TJob : IJob
    {
        if (!Enum.TryParse<JobTypes>(jobKey.Group, out _))
            return Result.Fail($"Quartz job group '{jobKey.Group}' is not a valid {nameof(JobTypes)} value");

        if (await scheduler.CheckExists(jobKey, cancellationToken))
            return Result.Ok(DateTimeOffset.MinValue);

        var job = JobBuilder
            .Create<TJob>()
            .WithIdentity(jobKey)
            .UsingJobData(jobDataMap)
            .DisallowConcurrentExecution()
            .RequestRecovery()
            .Build();
        var triggerBuilder = TriggerBuilder.Create().WithIdentity(jobKey.Name, jobKey.Group).ForJob(job);
        triggerBuilder = executeAt.HasValue ? triggerBuilder.StartAt(executeAt.Value) : triggerBuilder.StartNow();
        var trigger = triggerBuilder.WithSimpleSchedule(x => x.WithMisfireHandlingInstructionFireNow()).Build();

        try
        {
            return Result.Ok(await scheduler.ScheduleJob(job, trigger, cancellationToken));
        }
        catch (ObjectAlreadyExistsException)
        {
            return Result.Ok(DateTimeOffset.MinValue);
        }
    }

    public static async Task<Result> ExecuteJobs<TJob>(
        this IScheduler scheduler,
        IReadOnlyCollection<(JobKey Key, JobDataMap DataMap)> jobs,
        CancellationToken cancellationToken = default,
        DateTimeOffset? executeAt = null
    )
        where TJob : IJob
    {
        var jobsAndTriggers = new Dictionary<IJobDetail, IReadOnlyCollection<ITrigger>>();
        foreach (var (key, dataMap) in jobs)
        {
            if (await scheduler.CheckExists(key, cancellationToken))
                continue;

            var detail = JobBuilder
                .Create<TJob>()
                .WithIdentity(key)
                .UsingJobData(dataMap)
                .DisallowConcurrentExecution()
                .RequestRecovery()
                .Build();
            var triggerBuilder = TriggerBuilder.Create().WithIdentity(key.Name, key.Group).ForJob(detail);
            triggerBuilder = executeAt.HasValue ? triggerBuilder.StartAt(executeAt.Value) : triggerBuilder.StartNow();
            jobsAndTriggers.Add(
                detail,
                [triggerBuilder.WithSimpleSchedule(x => x.WithMisfireHandlingInstructionFireNow()).Build()]
            );
        }

        if (jobsAndTriggers.Count == 0)
            return Result.Ok();

        await scheduler.ScheduleJobs(jobsAndTriggers, replace: false, cancellationToken);
        return Result.Ok();
    }

    public static Task<bool> IsJobRunning(
        this IScheduler scheduler,
        JobKey key,
        CancellationToken cancellationToken = default
    ) => scheduler.IsJobExecuting(key, cancellationToken);

    public static Task<bool> IsQueued(
        this IScheduler scheduler,
        JobKey key,
        CancellationToken cancellationToken = default
    ) => scheduler.IsQueuedInternal(key, cancellationToken);

    public static async Task<bool> IsJobExecuting(
        this IScheduler scheduler,
        JobKey key,
        CancellationToken cancellationToken = default
    ) => (await scheduler.GetCurrentlyExecutingJobs(cancellationToken)).Any(x => x.JobDetail.Key.Equals(key));

    public static async Task<Result> CancelJob(
        this IScheduler scheduler,
        JobKey key,
        CancellationToken cancellationToken = default,
        bool waitForCompletion = true
    )
    {
        var executing = await scheduler.IsJobExecuting(key, cancellationToken);
        if (executing)
            await scheduler.Interrupt(key, cancellationToken);
        if (await scheduler.CheckExists(key, cancellationToken))
            await scheduler.DeleteJob(key, cancellationToken);

        if (executing && waitForCompletion)
            await scheduler.AwaitJobCompletion(key, cancellationToken);

        return Result.Ok();
    }

    public static async Task<Result> DeleteBatchJobs(
        this IScheduler scheduler,
        IReadOnlyCollection<JobKey> keys,
        CancellationToken cancellationToken = default
    )
    {
        await scheduler.DeleteJobs(keys.ToList(), cancellationToken);
        return Result.Ok();
    }

    public static async Task AwaitJobCompletion(
        this IScheduler scheduler,
        JobKey key,
        CancellationToken cancellationToken = default,
        int timeoutSeconds = 30
    )
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));
        try
        {
            while (await scheduler.IsJobExecuting(key, timeout.Token))
                await Task.Delay(200, timeout.Token);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested) { }
    }

    public static async Task<IReadOnlyCollection<IJobExecutionContext>> GetActiveJobs(
        this IScheduler scheduler,
        CancellationToken cancellationToken = default
    ) => await scheduler.GetCurrentlyExecutingJobs(cancellationToken);

    public static async Task<bool> IsActive(
        this IScheduler scheduler,
        JobKey key,
        CancellationToken cancellationToken = default
    ) => await scheduler.IsQueued(key, cancellationToken) || await scheduler.IsJobExecuting(key, cancellationToken);

    public static Task WaitForJobsToFinish(
        this IScheduler scheduler,
        IEnumerable<JobKey> keys,
        TimeSpan timeout,
        CancellationToken cancellationToken = default
    ) =>
        Task.WhenAll(
            keys.Distinct()
                .Select(key =>
                    scheduler.AwaitJobCompletion(key, cancellationToken, (int)Math.Ceiling(timeout.TotalSeconds))
                )
        );

    public static async Task AwaitScheduler(this IScheduler scheduler, CancellationToken cancellationToken = default)
    {
        var timeoutAt = DateTime.UtcNow.AddSeconds(30);
        while (DateTime.UtcNow < timeoutAt)
        {
            if ((await scheduler.GetCurrentlyExecutingJobs(cancellationToken)).Count == 0)
                return;

            await Task.Delay(100, cancellationToken);
        }
    }

    public static async Task<IReadOnlyCollection<JobKey>> GetJobKeys(
        this IScheduler scheduler,
        JobTypes jobType,
        CancellationToken cancellationToken = default
    ) => await scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(jobType.ToString()), cancellationToken);

    public static async Task<bool> HasActiveJobs(
        this IScheduler scheduler,
        IEnumerable<JobKey> keys,
        CancellationToken cancellationToken = default
    )
    {
        foreach (var key in keys)
        {
            if (await scheduler.IsActive(key, cancellationToken))
                return true;
        }

        return false;
    }

    public static async Task<List<JobStatusUpdate<string>>> GetRunningJobUpdates(this IScheduler scheduler)
    {
        return (await scheduler.GetCurrentlyExecutingJobs())
            .Select(context => new JobStatusUpdate<string>(
                JobStatusUpdateMapper.ToJobType(context.JobDetail.Key.Group),
                JobStatus.Started,
                JsonSerializer.Serialize(context.MergedJobDataMap, DefaultJsonSerializerOptions.ConfigStandard),
                context.JobDetail.Key.Name,
                context.FireTimeUtc.UtcDateTime
            ))
            .ToList();
    }

    private static async Task<bool> IsQueuedInternal(
        this IScheduler scheduler,
        JobKey key,
        CancellationToken cancellationToken
    )
    {
        if (!await scheduler.CheckExists(key, cancellationToken))
            return false;

        foreach (var trigger in await scheduler.GetTriggersOfJob(key, cancellationToken))
        {
            if (trigger is ICronTrigger)
                continue;

            var state = await scheduler.GetTriggerState(trigger.Key, cancellationToken);
            if (state is TriggerState.Normal or TriggerState.Blocked or TriggerState.Paused)
                return true;
        }

        return false;
    }
}
