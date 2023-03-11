using Data.Contracts;
using DownloadManager.Contracts;
using Logging.Interface;
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
    private readonly Func<DownloadWorkerTask, DownloadWorker> _downloadWorkerFactory;

    private readonly List<DownloadWorker> _downloadWorkers = new();

    private readonly IServerSettingsModule _serverSettings;

    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private CancellationToken _cancellationToken => _cancellationTokenSource.Token;

    private IDisposable _downloadSpeedLimitSubscription;

    private bool _isStopped = false;

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
        Func<DownloadWorkerTask, DownloadWorker> downloadWorkerFactory,
        IServerSettingsModule serverSettings)
    {
        _log = log;
        _mediator = mediator;
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

        // SetupSubscriptions();
        DownloadTask = downloadTask;

        return Result.Ok();
    }

    public async Task<Result<DownloadTask>> PauseAsync()
    {
        if (DownloadStatus != DownloadStatus.Downloading)
            _log.Warning("DownloadClient with {DownloadTaskFileName} is currently not downloading and cannot be paused", DownloadTask.FileName);

        _log.Information("Pause downloading of {DownloadTaskFileName}", DownloadTask.FileName);

        await Task.WhenAll(_downloadWorkers.Select(x => x.PauseAsync()));

        DownloadTask.DownloadWorkerTasks = _downloadWorkers.Select(x => x.DownloadWorkerTask).ToList();
        return Result.Ok(DownloadTask);
    }

    /// <summary>
    /// Starts the download workers for the <see cref="DownloadTask"/> given during setup.
    /// </summary>
    /// <returns>Is successful.</returns>
    public async Task<Result> Start()
    {
        if (_downloadWorkers.Any(x => x.DownloadWorkerTask.DownloadStatus == DownloadStatus.Downloading))
            return Result.Fail("The PlexDownloadClient is already downloading and can not be started.").LogWarning();

        _log.Debug("Start downloading {FileName}", DownloadTask.FileName);
        try
        {
            var results = new List<Result>();
            foreach (var downloadWorker in _downloadWorkers)
            {
                var startResult = await downloadWorker.Start();
                if (startResult.IsFailed)
                    startResult.LogError();

                results.Add(startResult);
            }

            DownloadProcessTask = Task.WhenAll(_downloadWorkers
                .Select(x => x.DownloadProcessTask)
                .Append(ExecuteDownloadWorkerUpdates(_cancellationToken)));

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

        _isStopped = true;

        // We Await the DownloadProcessTask to ensure that the DownloadWorkerUpdates are completed
        await DownloadProcessTask;

        return Result.Ok();
    }

    /// <summary>
    /// Releases the unmanaged resources used by the HttpClient and optionally disposes of the managed resources.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        _downloadSpeedLimitSubscription?.Dispose();
        _cancellationTokenSource.Cancel();
        await DownloadProcessTask;

        _log.Here().Warning("Disposing PlexDownloadClient for DownloadTask with Id: {DownloadTaskId}", DownloadTask.Id);
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

        await _mediator.Send(new UpdateDownloadTasksByIdCommand(DownloadTask), cancellationToken);

        await _mediator.Send(new DownloadTaskUpdated(DownloadTask), cancellationToken);

        _log.Debug("{@DownloadTask}", DownloadTask.ToString());
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

    private async Task<Result> ExecuteDownloadWorkerUpdates(CancellationToken cancellationToken = default)
    {
        async Task<bool> ExecuteUpdate()
        {
            var list = _downloadWorkers.Select(x => x.DownloadWorkerTask).ToList();
            if (list.Any())
            {
                await OnDownloadWorkerTaskUpdate(list, cancellationToken);

                if (list.All(x => x.DownloadStatus == DownloadStatus.DownloadFinished))
                {
                    await _mediator.Publish(new DownloadTaskFinished(DownloadTask.Id, DownloadTask.PlexServerId), cancellationToken);
                    return true;
                }

                if (list.Any(x => x.DownloadStatus == DownloadStatus.Error))
                {
                    // TODO Handle error
                    return true;
                }
            }

            return false;
        }

        while (!_isStopped)
        {
            try
            {
                var shouldBreak = await ExecuteUpdate();

                if (shouldBreak || cancellationToken.IsCancellationRequested)
                    break;

                await Task.Delay(1000, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }

        await ExecuteUpdate();
        _log.DebugLine("ExecuteDownloadWorkerUpdates has finished");

        return Result.Ok();
    }

    #endregion
}