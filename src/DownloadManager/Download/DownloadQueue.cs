using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Channels;
using BackgroundServices.Contracts;
using Data.Contracts;
using DownloadManager.Contracts;
using Logging.Interface;

namespace PlexRipper.DownloadManager;

/// <summary>
/// The DownloadQueue is responsible for deciding which downloadTask is handled by the <see cref="DownloadManager"/>.
/// </summary>
public class DownloadQueue : IDownloadQueue
{
    #region Fields

    private readonly ILog _log;
    private readonly IMediator _mediator;
    private readonly IDownloadTaskScheduler _downloadTaskScheduler;

    private readonly Channel<int> _plexServersToCheckChannel = Channel.CreateUnbounded<int>();

    private readonly Subject<int> _serverCompletedDownloading = new();

    private readonly CancellationToken _token = new();

    private Task<Task> _copyTask;

    #endregion

    #region Constructor

    public DownloadQueue(ILog log, IMediator mediator, IDownloadTaskScheduler downloadTaskScheduler)
    {
        _log = log;
        _mediator = mediator;
        _downloadTaskScheduler = downloadTaskScheduler;
    }

    #endregion

    #region Properties

    public bool IsBusy => _plexServersToCheckChannel.Reader.Count > 0;

    /// <summary>
    /// Emits the id of a <see cref="PlexServer"/> which has no more <see cref="DownloadTask">DownloadTasks</see> to process.
    /// </summary>
    public IObservable<int> ServerCompletedDownloading => _serverCompletedDownloading.AsObservable();

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

        Log.Information($"Adding {plexServerIds.Count} {nameof(PlexServer)}s to the DownloadQueue to check for the next download.");
        foreach (var plexServerId in plexServerIds)
            await _plexServersToCheckChannel.Writer.WriteAsync(plexServerId, _token);

        return Result.Ok();
    }

    public async Task<Result<DownloadTask>> CheckDownloadQueueServer(int plexServerId)
    {
        if (plexServerId <= 0)
            return ResultExtensions.IsInvalidId(nameof(plexServerId), plexServerId).LogWarning();

        var downloadTasksResult = await _mediator.Send(new GetDownloadTasksByPlexServerIdQuery(plexServerId));
        if (downloadTasksResult.IsFailed)
            return downloadTasksResult.LogError();

        var plexServerName = await _mediator.Send(new GetPlexServerNameByIdQuery(plexServerId));
        if (plexServerName.IsFailed)
            return plexServerName.LogError();

        _log.Debug("Checking {NameOfPlexServer)}: {PlexServerName} for the next download to start", nameof(PlexServer), plexServerName.Value, 0);

        var nextDownloadTaskResult = GetNextDownloadTask(downloadTasksResult.Value);
        if (nextDownloadTaskResult.IsFailed)
        {
            _log.Information("There are no available downloadTasks remaining for PlexServer with Id: {PlexServerName}", plexServerName.Value);
            _serverCompletedDownloading.OnNext(plexServerId);
            return Result.Ok();
        }

        var nextDownloadTask = nextDownloadTaskResult.Value;

        _log.Information("Selected download task {NextDownloadTaskFullTitle} to start as the next task", nextDownloadTask.FullTitle);

        await _downloadTaskScheduler.StartDownloadTaskJob(nextDownloadTask.Id, nextDownloadTask.PlexServerId);

        return Result.Ok(nextDownloadTask);
    }

    /// <summary>
    ///  Determines the next downloadable <see cref="DownloadTask"/>.
    /// Will only return a successful result if a queued task can be found
    /// </summary>
    /// <param name="downloadTasks"></param>
    /// <returns></returns>
    public Result<DownloadTask> GetNextDownloadTask(List<DownloadTask> downloadTasks)
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

            return Result.Fail($"DownloadTask {nextDownloadTask.Title} is already downloading").LogDebug();
        }

        nextDownloadTask = downloadTasks.FirstOrDefault(x => x.DownloadStatus == DownloadStatus.Queued);
        if (nextDownloadTask is null)
            return Result.Fail("There were no downloadTasks left to download.").LogDebug();

        // Should we check deeper for any nested queued tasks in downloading tasks
        if (nextDownloadTask.Children is not null && nextDownloadTask.Children.Any())
        {
            var children = nextDownloadTask.Children;
            return GetNextDownloadTask(children);
        }

        return Result.Ok(nextDownloadTask);
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