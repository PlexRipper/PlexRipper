using System.Threading.Channels;
using Application.Contracts;
using Data.Contracts;
using Logging.Interface;

namespace PlexRipper.Application;

/// <summary>
/// The DownloadQueue is responsible for deciding which downloadTask is handled by the <see cref="DownloadManager"/>.
/// </summary>
public class DownloadQueue : IDownloadQueue
{
    #region Fields

    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;
    private readonly IDownloadTaskScheduler _downloadTaskScheduler;

    private readonly Channel<int> _plexServersToCheckChannel = Channel.CreateUnbounded<int>();

    private readonly CancellationToken _token = new();

    private Task<Task> _copyTask;

    #endregion

    #region Constructor

    public DownloadQueue(ILog log, IPlexRipperDbContext dbContext, IDownloadTaskScheduler downloadTaskScheduler)
    {
        _log = log;
        _dbContext = dbContext;
        _downloadTaskScheduler = downloadTaskScheduler;
    }

    #endregion

    #region Properties

    public bool IsBusy => _plexServersToCheckChannel.Reader.Count > 0;

    #endregion

    #region Public Methods

    public Result Setup()
    {
        _copyTask = Task.Factory.StartNew(ExecuteDownloadQueueCheck, TaskCreationOptions.LongRunning);
        return _copyTask.IsFaulted ? Result.Fail("ExecuteFileTasks failed due to an error").LogError() : Result.Ok();
    }

    /// <summary>
    /// Check the DownloadQueue for downloadTasks which can be started.
    /// </summary>
    public async Task<Result> CheckDownloadQueue(List<int> plexServerIds)
    {
        if (!plexServerIds.Any())
            return ResultExtensions.IsEmpty(nameof(plexServerIds)).LogWarning();

        _log.Here()
            .Information("Adding {PlexServerIdsCount} {NameOfPlexServer}s to the DownloadQueue to check for the next download", plexServerIds.Count,
                nameof(PlexServer));
        foreach (var plexServerId in plexServerIds)
            await _plexServersToCheckChannel.Writer.WriteAsync(plexServerId, _token);

        return Result.Ok();
    }

    internal async Task<Result<DownloadTaskGeneric>> CheckDownloadQueueServer(int plexServerId)
    {
        if (plexServerId <= 0)
            return ResultExtensions.IsInvalidId(nameof(plexServerId), plexServerId).LogWarning();

        var plexServerName = await _dbContext.GetPlexServerNameById(plexServerId, _token);
        var downloadTasks = await _dbContext.GetAllDownloadTasksAsync(plexServerId, cancellationToken: _token);

        _log.Here().Debug("Checking {NameOfPlexServer}: {PlexServerName} for the next download to start", nameof(PlexServer), plexServerName);
        var nextDownloadTaskResult = GetNextDownloadTask(downloadTasks);
        if (nextDownloadTaskResult.IsFailed)
        {
            _log.Information("There are no available downloadTasks remaining for PlexServer with Id: {PlexServerName}", plexServerName);
            return Result.Ok();
        }

        var nextDownloadTask = nextDownloadTaskResult.Value;

        _log.Information("Selected download task {NextDownloadTaskFullTitle} to start as the next task", nextDownloadTask.FullTitle);

        await _downloadTaskScheduler.StartDownloadTaskJob(nextDownloadTask.ToKey());

        return Result.Ok(nextDownloadTask);
    }

    /// <summary>
    ///  Determines the next downloadable <see cref="DownloadTaskGeneric"/>.
    /// Will only return a successful result if a queued task can be found
    /// </summary>
    /// <param name="downloadTasks"></param>
    /// <returns></returns>
    internal Result<DownloadTaskGeneric> GetNextDownloadTask(List<DownloadTaskGeneric> downloadTasks)
    {
        // Check if there is anything downloading already
        var nextDownloadTask = downloadTasks.FirstOrDefault(x => x.DownloadStatus == DownloadStatus.Downloading);
        if (nextDownloadTask is not null)
        {
            // Should we check deeper for any nested queued tasks inside downloading tasks
            if (nextDownloadTask.Children is not null && nextDownloadTask.Children.Any())
            {
                var children = nextDownloadTask.Children;
                return GetNextDownloadTask(children);
            }

            return Result.Fail($"DownloadTask {nextDownloadTask.FullTitle} is already downloading").LogDebug();
        }

        // Check if there is anything queued
        nextDownloadTask = downloadTasks.FirstOrDefault(x => x.DownloadStatus == DownloadStatus.Queued);
        if (nextDownloadTask is not null)
        {
            if (nextDownloadTask.Children is not null && nextDownloadTask.Children.Any())
            {
                var children = nextDownloadTask.Children;
                return GetNextDownloadTask(children);
            }

            return Result.Ok(nextDownloadTask);
        }

        return Result.Fail("There were no downloadTasks left to download.").LogDebug();
    }

    #endregion

    #region Private Methods

    private async Task ExecuteDownloadQueueCheck()
    {
        while (!_token.IsCancellationRequested)
        {
            var item = await _plexServersToCheckChannel.Reader.ReadAsync(_token);
            await CheckDownloadQueueServer(item);
        }
    }

    #endregion
}