using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Data.Contracts;
using DownloadManager.Contracts;
using Logging.Interface;
using PlexRipper.Domain.RxNet;
using Settings.Contracts;

namespace PlexRipper.DownloadManager;

/// <summary>
/// The PlexDownloadClient handles a single <see cref="DownloadTask"/> at a time and
/// manages the <see cref="DownloadWorker"/>s responsible for the multi-threaded downloading.
/// </summary>
public class PlexDownloadClient : IDisposable
{
    #region Fields

    private readonly ILog _log;
    private readonly IMediator _mediator;
    private readonly Func<DownloadWorkerTask, DownloadWorker> _downloadWorkerFactory;

    private readonly List<DownloadWorker> _downloadWorkers = new();

    private readonly EventLoopScheduler _timeThreadContext = new();

    private readonly IServerSettingsModule _serverSettings;

    private IDisposable _downloadSpeedLimitSubscription;

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
    public Task DownloadProcessTask => Task.WhenAll(_downloadWorkers.Select(x => x.DownloadProcessTask ?? Task.CompletedTask));

    public DownloadStatus DownloadStatus
    {
        get => DownloadTask.DownloadStatus;
        internal set => DownloadTask.DownloadStatus = value;
    }

    public DownloadTask DownloadTask { get; internal set; }

    /// <summary>
    /// The ClientId/DownloadTaskId is always the same id that is assigned to the <see cref="DownloadTask"/>.
    /// </summary>
    public int DownloadTaskId => DownloadTask.Id;

    #endregion

    #region Public Methods

    /// <summary>
    /// Setup this <see cref="PlexDownloadClient"/> to prepare for the download process.
    /// This needs to be called before any other action can be taken.
    /// Note: adding this in the constructor prevents us from returning a <see cref="Result"/>.
    /// </summary>
    /// <param name="downloadTask">The <see cref="DownloadTask"/> to start executing.</param>
    /// <returns></returns>
    public Result<PlexDownloadClient> Setup(DownloadTask downloadTask)
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
        SetupSubscriptions();

        DownloadTask = downloadTask;
        return Result.Ok(this);
    }

    public async Task<Result<DownloadTask>> PauseAsync()
    {
        if (DownloadStatus != DownloadStatus.Downloading)
            Log.Warning($"DownloadClient with {DownloadTask.FileName} is currently not downloading and cannot be paused.");

        _log.Information("Pause downloading of {DownloadTaskFileName}", DownloadTask.FileName);

        await Task.WhenAll(_downloadWorkers.Select(x => x.PauseAsync()));

        DownloadTask.DownloadWorkerTasks = _downloadWorkers.Select(x => x.DownloadWorkerTask).ToList();
        return Result.Ok(DownloadTask);
    }

    /// <summary>
    /// Starts the download workers for the <see cref="DownloadTask"/> given during setup.
    /// </summary>
    /// <returns>Is successful.</returns>
    public Result Start()
    {
        if (_downloadWorkers.Any(x => x.DownloadWorkerTask.DownloadStatus == DownloadStatus.Downloading))
            return Result.Fail("The PlexDownloadClient is already downloading and can not be started.").LogWarning();

        _log.Debug("Start downloading {FileName}", DownloadTask.FileName);
        try
        {
            var results = new List<Result>();
            foreach (var downloadWorker in _downloadWorkers)
            {
                var startResult = downloadWorker.Start();
                if (startResult.IsFailed)
                    startResult.LogError();

                results.Add(startResult);
            }

            return results.Merge();
        }
        catch (Exception e)
        {
            return Result.Fail(new ExceptionalError($"Could not download {DownloadTask.FileName}", e)).LogError();
        }
    }

    public async Task<Result<DownloadTask>> StopAsync()
    {
        _log.Information("Stop downloading {DownloadTaskFileName} from {DownloadTaskDownloadUrl}", DownloadTask.FileName, DownloadTask.DownloadUrl, 0);

        await Task.WhenAll(_downloadWorkers.Select(x => x.StopAsync()));

        DownloadTask.DownloadWorkerTasks = _downloadWorkers.Select(x => x.DownloadWorkerTask).ToList();
        DownloadTask.DownloadStatus = DownloadStatus.Stopped;
        return Result.Ok(DownloadTask);
    }

    /// <summary>
    /// Releases the unmanaged resources used by the HttpClient and optionally disposes of the managed resources.
    /// </summary>
    public void Dispose()
    {
        _timeThreadContext.Dispose();
        ClearDownloadWorkers();
        _downloadSpeedLimitSubscription?.Dispose();
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Calls Dispose on all DownloadWorkers and clears the downloadWorkersList.
    /// </summary>
    /// <returns>Is successful.</returns>
    private void ClearDownloadWorkers()
    {
        if (_downloadWorkers.Any())
        {
            _downloadWorkers.ForEach(x => x.Dispose());
            _downloadWorkers.Clear();
        }

        if (DownloadTask is not null)
            _log.Debug("DownloadWorkers have been disposed for {DownloadTaskFullTitle}", DownloadTask.FullTitle);
    }

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

    private async Task OnDownloadWorkerTaskUpdate(IList<DownloadWorkerTask> downloadWorkerUpdates)
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

        DownloadTask.DataReceived = DownloadTask.DownloadWorkerTasks.Sum(x => x.BytesReceived);
        DownloadTask.Percentage = DownloadTask.DownloadWorkerTasks.Average(x => x.Percentage);
        DownloadTask.DownloadSpeed = DownloadTask.DownloadWorkerTasks.Sum(x => x.DownloadSpeed);

        var newDownloadStatus = DownloadTaskActions.Aggregate(downloadWorkerUpdates.Select(x => x.DownloadStatus).ToList());

        var statusIsChanged = DownloadStatus != newDownloadStatus;
        DownloadStatus = newDownloadStatus;

        await _mediator.Send(new UpdateDownloadTasksByIdCommand(new List<DownloadTask> { DownloadTask }));

        if (statusIsChanged)
        {
            await _mediator.Publish(new DownloadStatusChanged(
                DownloadTask.Id,
                DownloadTask.RootDownloadTaskId,
                DownloadStatus));
        }
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

    private void SetupSubscriptions()
    {
        if (!_downloadWorkers.Any())
        {
            Log.Warning("No download workers have been made yet, cannot setup subscriptions.");
            return;
        }

        // On download worker update
        _downloadWorkers
            .Select(x => x.DownloadWorkerTaskUpdate)
            .CombineLatest()
            .SubscribeAsync(OnDownloadWorkerTaskUpdate);

        // Download Worker Log subscription
        _downloadWorkers
            .Select(x => x.DownloadWorkerLog)
            .Merge()
            .Buffer(TimeSpan.FromSeconds(1))
            .SubscribeAsync(async logs => await _mediator.Send(new AddDownloadWorkerLogsCommand(logs)));
    }

    #endregion
}