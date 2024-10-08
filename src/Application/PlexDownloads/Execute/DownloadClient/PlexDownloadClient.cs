using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Application.Contracts;
using Data.Contracts;
using Logging.Interface;
using Settings.Contracts;

namespace PlexRipper.Application;

/// <summary>
/// The PlexDownloadClient handles a single <see cref="DownloadTaskGeneric"/> at a time and
/// manages the <see cref="DownloadWorker"/>s responsible for the multi-threaded downloading.
/// </summary>
public class PlexDownloadClient : IAsyncDisposable, IPlexDownloadClient
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
        IServerSettingsModule serverSettings
    )
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
    public Task DownloadProcessTask { get; private set; }

    public DownloadStatus DownloadStatus
    {
        get => DownloadTask.DownloadStatus;
        internal set => DownloadTask.DownloadStatus = value;
    }

    /// <summary>
    /// Gets the <see cref="DownloadTaskGeneric"/> that is currently being executed.
    /// </summary>
    public DownloadTaskGeneric DownloadTask { get; internal set; }

    public IObservable<IList<DownloadWorkerLog>> ListenToDownloadWorkerLog { get; private set; }

    #endregion

    #region Public Methods

    /// <summary>
    /// Setup this <see cref="PlexDownloadClient"/> to prepare for the download process.
    /// This needs to be called before any other action can be taken.
    /// Note: adding this in the constructor prevents us from returning a <see cref="Result"/>.
    /// </summary>
    /// <returns></returns>
    public async Task<Result> Setup(DownloadTaskKey downloadTaskKey, CancellationToken cancellationToken = default)
    {
        var downloadTask = await _dbContext.GetDownloadTaskAsync(downloadTaskKey, cancellationToken);
        if (downloadTask is null)
        {
            return ResultExtensions
                .EntityNotFound(nameof(DownloadTaskGeneric), downloadTaskKey.ToString())
                .LogWarning();
        }

        DownloadTask = downloadTask;

        if (!DownloadTask.DownloadWorkerTasks.Any())
        {
            return ResultExtensions
                .IsEmpty($"{nameof(DownloadTaskGeneric)}.{nameof(DownloadTask.DownloadWorkerTasks)}")
                .LogWarning();
        }

        _downloadWorkers.AddRange(
            DownloadTask.DownloadWorkerTasks.Select(downloadWorkerTask => _downloadWorkerFactory(downloadWorkerTask))
        );

        await SetupDownloadLimitWatcher(DownloadTask);

        SetupSubscriptions();

        return Result.Ok();
    }

    /// <summary>
    /// Starts the download workers for the <see cref="DownloadTaskGeneric"/> given during setup.
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

            DownloadProcessTask = Task.WhenAll(
                _downloadWorkers
                    .Select(x => x.DownloadProcessTask)
                    .Concat(
                        new Task[]
                        {
                            _downloadWorkerTaskUpdateCompletionSource.Task,
                            _downloadWorkerLogCompletionSource.Task,
                        }
                    )
            );

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
        if (DownloadTask is not null)
        {
            _log.Here()
                .Warning("PlexDownloadClient for DownloadTask with Id: {DownloadTaskId} was disposed", DownloadTask.Id);
        }
    }

    #endregion

    #region Private Methods

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

        DownloadTask.DataReceived = downloadWorkerUpdates.Sum(x => x.BytesReceived);
        DownloadTask.Percentage = downloadWorkerUpdates.Average(x => x.Percentage);
        DownloadTask.DownloadSpeed = downloadWorkerUpdates.Sum(x => x.DownloadSpeed);

        var newDownloadStatus = DownloadTaskActions.Aggregate(
            downloadWorkerUpdates.Select(x => x.DownloadStatus).ToList()
        );

        DownloadStatus = newDownloadStatus;

        await _dbContext.UpdateDownloadProgress(DownloadTask.ToKey(), DownloadTask);
        await _dbContext.SetDownloadStatus(DownloadTask.ToKey(), DownloadStatus);

        await _mediator.Send(new DownloadTaskUpdatedNotification(DownloadTask.ToKey()));

        _log.Debug("{@DownloadTask}", DownloadTask.ToString());
    }

    private async Task SetupDownloadLimitWatcher(DownloadTaskGeneric downloadTask)
    {
        void SetDownloadSpeedLimit(int downloadSpeedLimitInKb)
        {
            foreach (var downloadWorker in _downloadWorkers)
                downloadWorker.SetDownloadSpeedLimit(downloadSpeedLimitInKb / _downloadWorkers.Count);
        }

        var serverMachineIdentifier = await _dbContext.GetPlexServerMachineIdentifierById(downloadTask.PlexServerId);

        SetDownloadSpeedLimit(_serverSettings.GetDownloadSpeedLimit(serverMachineIdentifier));
        _downloadSpeedLimitSubscription = _serverSettings
            .GetDownloadSpeedLimitObservable(serverMachineIdentifier)
            .Subscribe(SetDownloadSpeedLimit);
    }

    private void SetupSubscriptions()
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
            .SelectMany(async data => await OnDownloadWorkerTaskUpdate(data).ToObservable())
            .Subscribe(
                _ => { },
                ex =>
                {
                    if (ex.GetType() != typeof(OperationCanceledException))
                    {
                        _log.Error(ex);
                        _downloadWorkerTaskUpdateCompletionSource.SetException(ex);
                    }

                    _downloadWorkerTaskUpdateCompletionSource.SetResult(true);
                },
                () => _downloadWorkerTaskUpdateCompletionSource.SetResult(true)
            );

        // Download Worker Log subscription
        ListenToDownloadWorkerLog = _downloadWorkers
            .Select(x => x.DownloadWorkerLog)
            .Merge()
            .Buffer(TimeSpan.FromSeconds(1))
            .AsObservable();

        // Complete the DownloadTask when this completes
        ListenToDownloadWorkerLog.Subscribe(
            _ => { },
            ex =>
            {
                if (ex.GetType() != typeof(OperationCanceledException))
                {
                    _log.Error(ex);
                    _downloadWorkerLogCompletionSource.SetException(ex);
                }

                _downloadWorkerLogCompletionSource.SetResult(true);
            },
            () => _downloadWorkerLogCompletionSource.SetResult(true)
        );
    }

    #endregion
}
