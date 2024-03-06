using BackgroundServices.Contracts;
using Data.Contracts;
using FileSystem.Contracts;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace PlexRipper.FileSystem;

public class FileMergeScheduler : BaseScheduler, IFileMergeScheduler
{
    private readonly IPlexRipperDbContext _dbContext;

    public FileMergeScheduler(ILog log, IScheduler scheduler, IPlexRipperDbContext dbContext) : base(log, scheduler)
    {
        _dbContext = dbContext;
    }

    protected override JobKey DefaultJobKey => new($"DownloadTaskId_", nameof(FileMergeJob));

    /// <summary>
    /// Creates an FileTask from a completed <see cref="DownloadTaskGeneric"/> and adds this to the database.
    /// </summary>
    /// <param name="downloadTaskKey"></param>
    public async Task<Result<FileTask>> CreateFileTaskFromDownloadTask(DownloadTaskKey downloadTaskKey)
    {
        var downloadTask = await _dbContext.GetDownloadTaskAsync(downloadTaskKey);
        if (downloadTask is null)
            return ResultExtensions.EntityNotFound(nameof(DownloadTaskGeneric), downloadTaskKey.Id);

        _log.Here().Debug("Adding DownloadTask \"{DownloadTaskTitle}\" with id {Id} to a FileTask to be merged", downloadTask.FullTitle, downloadTask.Id);

        await _dbContext.FileTasks.AddAsync(downloadTask.ToFileTask());
        _dbContext.SaveChanges();

        return await _dbContext.FileTasks.FirstOrDefaultAsync(x => x.DownloadTaskKey.Id == downloadTaskKey.Id);
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

        _log.Information("Stopping FileMergeJob for {NameOfDownloadFileTask)} with id: {FileTaskId}", nameof(FileTask), fileTaskId);

        var jobKey = FileMergeJob.GetJobKey(fileTaskId);
        if (!await IsJobRunning(jobKey))
            return Result.Fail($"{nameof(FileMergeJob)} with {jobKey} cannot be stopped because it is not running").LogWarning();

        return Result.OkIf(await StopJob(jobKey), $"Failed to stop {nameof(FileTask)} with id {fileTaskId}");
    }
}