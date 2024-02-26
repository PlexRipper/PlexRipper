using BackgroundServices.Contracts;
using Data.Contracts;
using FileSystem.Contracts;
using Logging.Interface;
using Quartz;

namespace PlexRipper.FileSystem;

public class FileMergeScheduler : BaseScheduler, IFileMergeScheduler
{
    private readonly IMediator _mediator;
    private readonly IPlexRipperDbContext _dbContext;

    public FileMergeScheduler(ILog log, IScheduler scheduler, IMediator mediator, IPlexRipperDbContext dbContext) : base(log, scheduler)
    {
        _mediator = mediator;
        _dbContext = dbContext;
    }

    protected override JobKey DefaultJobKey => new($"DownloadTaskId_", nameof(FileMergeJob));

    /// <summary>
    /// Creates an FileTask from a completed <see cref="DownloadTask"/> and adds this to the database.
    /// </summary>
    /// <param name="downloadTaskId"></param>
    public async Task<Result<DownloadFileTask>> CreateFileTaskFromDownloadTask(int downloadTaskId)
    {
        if (downloadTaskId == 0)
            return ResultExtensions.IsInvalidId(nameof(downloadTaskId)).LogError();

        var downloadTask = await _dbContext.DownloadTasks.IncludeDownloadTasks().GetAsync(downloadTaskId);
        if (downloadTask is null)
            return ResultExtensions.EntityNotFound(nameof(DownloadTask), downloadTaskId);

        _log.Here().Debug("Adding DownloadTask {DownloadTaskTitle} with id {Id} to a FileTask to be merged", downloadTask.Title, downloadTask.Id);
        var result = await _mediator.Send(new AddFileTaskFromDownloadTaskCommand(downloadTask));
        if (result.IsFailed)
            return result.ToResult().LogError();

        return await _mediator.Send(new GetFileTaskByIdQuery(result.Value));
    }

    public async Task<Result> StartFileMergeJob(int fileTaskId)
    {
        if (fileTaskId <= 0)
            return ResultExtensions.IsInvalidId(nameof(fileTaskId), fileTaskId).LogWarning();

        var jobKey = FileMergeJob.GetJobKey(fileTaskId);
        if (await IsJobRunning(jobKey))
            return Result.Fail($"{nameof(FileMergeJob)} with {jobKey} already exists").LogWarning();

        var job = JobBuilder.Create<FileMergeJob>()
            .UsingJobData(FileMergeJob.FileTaskId, fileTaskId)
            .WithIdentity(jobKey)
            .Build();

        var trigger = TriggerBuilder.Create()
            .WithIdentity($"{jobKey.Name}_trigger", jobKey.Group)
            .StartNow()
            .Build();

        await ScheduleJob(job, trigger);

        return Result.Ok();
    }

    public async Task<Result> StopFileMergeJob(int fileTaskId)
    {
        if (fileTaskId <= 0)
            return ResultExtensions.IsInvalidId(nameof(fileTaskId), fileTaskId).LogWarning();

        _log.Information("Stopping FileMergeJob for {NameOfDownloadFileTask)} with id: {FileTaskId}", nameof(DownloadFileTask), fileTaskId);

        var jobKey = FileMergeJob.GetJobKey(fileTaskId);
        if (!await IsJobRunning(jobKey))
            return Result.Fail($"{nameof(FileMergeJob)} with {jobKey} cannot be stopped because it is not running").LogWarning();

        return Result.OkIf(await StopJob(jobKey), $"Failed to stop {nameof(DownloadFileTask)} with id {fileTaskId}");
    }
}