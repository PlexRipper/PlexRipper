using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Data.Contracts;
using DownloadManager.Contracts;
using Logging.Interface;
using PlexRipper.Application;
using Settings.Contracts;

namespace PlexRipper.DownloadManager;

/// <summary>
/// The PlexDownloadClient handles a single <see cref="DownloadTask"/> at a time and
/// manages the <see cref="DownloadWorker"/>s responsible for the multi-threaded downloading.
/// </summary>
public class PlexDownloadClient : IAsyncDisposable
{
    #region Fields

    private readonly ILog _log;
    private readonly IMediator _mediator;
    private readonly IPlexRipperDbContext _dbContext;
    private readonly Func<DownloadWorkerTask, DownloadWorker> _downloadWorkerFactory;

    private readonly List<DownloadWorker> _downloadWorkers = new();

    private readonly IServerSettingsModule _serverSettings;

    private IDisposable _downloadSpeedLimitSubscription;
    private IDisposable _downloadWorkerTaskUpdate;
    private readonly TaskCompletionSource<object> _downloadWorkerTaskUpdateCompletionSource = new();
    private IDisposable _downloadWorkerLog;
    private readonly TaskCompletionSource<object> _downloadWorkerLogCompletionSource = new();

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="PlexDownloadClient"/> class.
    /// </summary>
    /// <param name="log"></param>
    /// <param name="mediator"></param>
    /// <param name="downloadWorkerFactory"></param>
    /// <param name="serverSettings"></param>
    public PlexDownloadClient(
        ILog log,
        IMediator mediator,
        IPlexRipperDbContext dbContext,
        Func<DownloadWorkerTask, DownloadWorker> downloadWorkerFactory,
        IServerSettingsModule serverSettings)
    {
        _log = log;
        _mediator = mediator;
        _dbContext = dbContext;
        _downloadWorkerFactory = downloadWorkerFactory;
        _serverSettings = serverSettings;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets the Task that completes when all download workers have finished.
    /// </summary>
    public Task DownloadProcessTask;

    public DownloadStatus DownloadStatus
    {
        get => DownloadTask.DownloadStatus;
        internal set => DownloadTask.DownloadStatus = value;
    }

    /// <summary>
    /// Gets the <see cref="DownloadTask"/> that is currently being executed.
    /// </summary>
    public DownloadTask DownloadTask { get; internal set; }

    #endregion

    #region Public Methods

    /// <summary>
    /// Setup this <see cref="PlexDownloadClient"/> to prepare for the download process.
    /// This needs to be called before any other action can be taken.
    /// Note: adding this in the constructor prevents us from returning a <see cref="Result"/>.
    /// </summary>
    /// <param name="downloadTask">The <see cref="DownloadTask"/> to start executing.</param>
    /// <returns></returns>
    public Result Setup(DownloadTask downloadTask)
    {
        if (downloadTask is null)
            return ResultExtensions.IsNull(nameof(downloadTask)).LogError();

        if (!downloadTask.DownloadWorkerTasks.Any())
            return ResultExtensions.IsEmpty(nameof(downloadTask.DownloadWorkerTasks)).LogError();

        if (downloadTask.PlexServer is null)
            return ResultExtensions.IsNull($"{nameof(downloadTask)}.{nameof(downloadTask.PlexServer)}").LogError();

        var createResult = CreateDownloadWorkers(downloadTask);
        if (createResult.IsFailed)
            return createResult.ToResult().LogError();

        SetupDownloadLimitWatcher(downloadTask);

        DownloadTask = downloadTask;

        return Result.Ok();
    }

    /// <summary>
    /// Starts the download workers for the <see cref="DownloadTask"/> given during setup.
    /// </summary>
    /// <returns>Is successful.</returns>
    public Result Start(CancellationToken cancellationToken = default)
    {
        if (_downloadWorkers.Any(x => x.DownloadWorkerTask.DownloadStatus == DownloadStatus.Downloading))
            return Result.Fail("The PlexDownloadClient is already downloading and can not be started.").LogWarning();

        _log.Debug("Start downloading {FileName}", DownloadTask.FileName);
        try
        {
            SetupSubscriptions(cancellationToken);

            var results = new List<Result>();
            foreach (var downloadWorker in _downloadWorkers)
            {
                var startResult = downloadWorker.Start();
                if (startResult.IsFailed)
                    startResult.LogError();

                results.Add(startResult);
            }

            DownloadProcessTask = Task.WhenAll(_downloadWorkers
                .Select(x => x.DownloadProcessTask)
                .Concat(new Task[] { _downloadWorkerTaskUpdateCompletionSource.Task, _downloadWorkerLogCompletionSource.Task }));

            return results.Merge();
        }
        catch (Exception e)
        {
            return Result.Fail(new ExceptionalError($"Could not download {DownloadTask.FileName}", e)).LogError();
        }
    }

    public async Task<Result> StopAsync()
    {
        _log.Here().Information("Stop downloading {DownloadTaskFileName}", DownloadTask.FileName);

        await Task.WhenAll(_downloadWorkers.Select(x => x.StopAsync()));

        // We Await the DownloadProcessTask to ensure that the DownloadWorkerUpdates are completed
        await DownloadProcessTask;
        await _downloadWorkerTaskUpdateCompletionSource.Task;
        await _downloadWorkerLogCompletionSource.Task;

        return Result.Ok();
    }

    /// <summary>
    /// Releases the unmanaged resources used by the HttpClient and optionally disposes of the managed resources.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (DownloadProcessTask is not null)
            await DownloadProcessTask;

        _downloadSpeedLimitSubscription?.Dispose();
        _downloadWorkerTaskUpdate?.Dispose();
        _downloadWorkerLog?.Dispose();
        if (DownloadTask is not null)
            _log.Here().Warning("PlexDownloadClient for DownloadTask with Id: {DownloadTaskId} was disposed", DownloadTask.Id);
    }

    #endregion

    #region Private Methods

    private Result<List<DownloadWorkerTask>> CreateDownloadWorkers(DownloadTask downloadTask)
    {
        if (downloadTask is null)
            return ResultExtensions.IsNull(nameof(downloadTask)).LogWarning();

        if (!downloadTask.DownloadWorkerTasks.Any())
            return ResultExtensions.IsEmpty($"{nameof(downloadTask)}.{nameof(downloadTask.DownloadWorkerTasks)}").LogWarning();

        _downloadWorkers.AddRange(downloadTask.DownloadWorkerTasks
            .Select(downloadWorkerTask => _downloadWorkerFactory(downloadWorkerTask)));

        return Result.Ok();
    }

    private async Task OnDownloadWorkerTaskUpdate(IList<DownloadWorkerTask> downloadWorkerUpdates, CancellationToken cancellationToken = default)
    {
        if (!downloadWorkerUpdates.Any())
            return;

        // Update every DownloadWorkerTask with the updated progress
        foreach (var downloadWorkerTask in downloadWorkerUpdates)
        {
            var i = DownloadTask.DownloadWorkerTasks.FindIndex(x => x.Id == downloadWorkerTask.Id);
            if (i > -1)
                DownloadTask.DownloadWorkerTasks[i] = downloadWorkerTask;
        }

        DownloadTask.DataReceived = downloadWorkerUpdates.Sum(x => x.BytesReceived);
        DownloadTask.Percentage = downloadWorkerUpdates.Average(x => x.Percentage);
        DownloadTask.DownloadSpeed = downloadWorkerUpdates.Sum(x => x.DownloadSpeed);

        var newDownloadStatus = DownloadTaskActions.Aggregate(downloadWorkerUpdates.Select(x => x.DownloadStatus).ToList());

        var statusIsChanged = DownloadStatus != newDownloadStatus;
        DownloadStatus = newDownloadStatus;

        await _dbContext.UpdateDownloadTasksAsync(DownloadTask, cancellationToken);

        await _mediator.Send(new DownloadTaskUpdated(DownloadTask), cancellationToken);

        _log.Debug("{@DownloadTask}", DownloadTask.ToString());

        if (statusIsChanged && DownloadStatus == DownloadStatus.DownloadFinished)
            await _mediator.Publish(new DownloadTaskFinished(DownloadTask.Id, DownloadTask.PlexServerId), cancellationToken);
    }

    private void SetupDownloadLimitWatcher(DownloadTask downloadTask)
    {
        void SetDownloadSpeedLimit(int downloadSpeedLimitInKb)
        {
            foreach (var downloadWorker in _downloadWorkers)
                downloadWorker.SetDownloadSpeedLimit(downloadSpeedLimitInKb / _downloadWorkers.Count);
        }

        SetDownloadSpeedLimit(_serverSettings.GetDownloadSpeedLimit(downloadTask.ServerMachineIdentifier));
        _downloadSpeedLimitSubscription = _serverSettings
            .GetDownloadSpeedLimitObservable(downloadTask.ServerMachineIdentifier)
            .Subscribe(SetDownloadSpeedLimit);
    }

    private void SetupSubscriptions(CancellationToken cancellationToken = default)
    {
        if (!_downloadWorkers.Any())
        {
            _log.WarningLine("No download workers have been made yet, cannot setup subscriptions");
            return;
        }

        // On download worker update
        _downloadWorkerTaskUpdate = _downloadWorkers
            .Select(x => x.DownloadWorkerTaskUpdate)
            .CombineLatest()
            .Sample(TimeSpan.FromSeconds(1))
            .SelectMany(async data => await OnDownloadWorkerTaskUpdate(data, cancellationToken).ToObservable())
            .Subscribe(
                _ => { },
                ex =>
                {
                    _log.Error(ex);
                    _downloadWorkerTaskUpdateCompletionSource.SetException(ex);
                },
                () => _downloadWorkerTaskUpdateCompletionSource.SetResult(true));

        // Download Worker Log subscription
        _downloadWorkerLog = _downloadWorkers
            .Select(x => x.DownloadWorkerLog)
            .Merge()
            .Buffer(TimeSpan.FromSeconds(1))
            .SelectMany(async logs => await InsertDownloadWorkerLogs(logs, cancellationToken)
                .ToObservable())
            .Subscribe(
                _ => { },
                ex =>
                {
                    _log.Error(ex);
                    _downloadWorkerLogCompletionSource.SetException(ex);
                },
                () => _downloadWorkerLogCompletionSource.SetResult(true));
    }

    private async Task InsertDownloadWorkerLogs(IList<DownloadWorkerLog> logs, CancellationToken cancellationToken = default)
    {
        if (logs is null || !logs.Any())
            return;

        try
        {
            await _dbContext.DownloadWorkerTasksLogs.AddRangeAsync(logs, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception e)
        {
            _log.Error(e);
        }
    }

    #endregion
}