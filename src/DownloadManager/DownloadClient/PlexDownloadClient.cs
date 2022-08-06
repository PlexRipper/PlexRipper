﻿using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using PlexRipper.Application;

namespace PlexRipper.DownloadManager.DownloadClient;

/// <summary>
/// The PlexDownloadClient handles a single <see cref="DownloadTask"/> at a time and
/// manages the <see cref="DownloadWorker"/>s responsible for the multi-threaded downloading.
/// </summary>
public class PlexDownloadClient : IDisposable
{
    #region Fields

    private readonly Subject<DownloadTask> _downloadTaskUpdate = new();

    private readonly Func<DownloadWorkerTask, DownloadWorker> _downloadWorkerFactory;

    private readonly Subject<DownloadWorkerLog> _downloadWorkerLog = new();

    private readonly List<DownloadWorker> _downloadWorkers = new();

    private readonly EventLoopScheduler _timeThreadContext = new();

    private readonly IServerSettingsModule _serverSettings;

    private IDisposable _downloadSpeedLimitSubscription;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="PlexDownloadClient"/> class.
    /// </summary>
    /// <param name="downloadWorkerFactory"></param>
    /// <param name="serverSettings"></param>
    public PlexDownloadClient(
        Func<DownloadWorkerTask, DownloadWorker> downloadWorkerFactory,
        IServerSettingsModule serverSettings)
    {
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

    public IObservable<DownloadTask> DownloadTaskUpdate => _downloadTaskUpdate.AsObservable();

    public IObservable<DownloadWorkerLog> DownloadWorkerLog => _downloadWorkerLog.AsObservable();

    #endregion

    #region Public Methods

    /// <summary>
    /// Setup this <see cref="PlexDownloadClient"/> to start execute work.
    /// This needs to be called before any other action can be taken.
    /// </summary>
    /// <param name="downloadTask">The <see cref="DownloadTask"/> to start executing.</param>
    /// <returns></returns>
    public Result<PlexDownloadClient> Setup(DownloadTask downloadTask)
    {
        if (downloadTask is null)
        {
            return ResultExtensions.IsNull(nameof(downloadTask));
        }

        if (!downloadTask.DownloadWorkerTasks.Any())
        {
            return ResultExtensions.IsEmpty(nameof(downloadTask.DownloadWorkerTasks));
        }

        if (DownloadTask.PlexServer is null)
        {
            return ResultExtensions.IsNull($"{nameof(DownloadTask)}.{nameof(DownloadTask.PlexServer)}");
        }

        DownloadTask = downloadTask;

        var createResult = CreateDownloadWorkers(downloadTask);
        if (createResult.IsFailed)
        {
            return createResult.ToResult().LogError();
        }

        SetupDownloadLimitWatcher();
        SetupSubscriptions();

        _downloadTaskUpdate.OnNext(DownloadTask);

        return Result.Ok(this);
    }

    public async Task<Result<DownloadTask>> PauseAsync()
    {
        if (DownloadStatus != DownloadStatus.Downloading)
        {
            Log.Warning($"DownloadClient with {DownloadTask.FileName} is currently not downloading and cannot be paused.");
        }

        Log.Information($"Pause downloading of {DownloadTask.FileName}");

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
        {
            return Result.Fail("The PlexDownloadClient is already downloading and can not be started.").LogWarning();
        }

        Log.Debug($"Start downloading {DownloadTask.FileName}");
        try
        {
            List<Result> results = new List<Result>();
            foreach (var downloadWorker in _downloadWorkers)
            {
                var startResult = downloadWorker.Start();
                if (startResult.IsFailed)
                {
                    startResult.LogError();
                }

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
        Log.Information($"Stop downloading {DownloadTask.FileName} from {DownloadTask.DownloadUrl}");

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
        {
            Log.Debug($"DownloadWorkers have been disposed for {DownloadTask.FullTitle}");
        }
    }

    private Result<List<DownloadWorkerTask>> CreateDownloadWorkers(DownloadTask downloadTask)
    {
        if (downloadTask is null)
        {
            return ResultExtensions.IsNull(nameof(downloadTask)).LogWarning();
        }

        if (!downloadTask.DownloadWorkerTasks.Any())
        {
            return ResultExtensions.IsEmpty($"{nameof(downloadTask)}.{nameof(downloadTask.DownloadWorkerTasks)}").LogWarning();
        }

        foreach (var downloadWorkerTask in downloadTask.DownloadWorkerTasks)
        {
            _downloadWorkers.Add(_downloadWorkerFactory(downloadWorkerTask));
        }

        return Result.Ok();
    }

    private void OnDownloadWorkerTaskUpdate(IList<DownloadWorkerTask> downloadWorkerUpdates)
    {
        if (_downloadTaskUpdate.IsDisposed || !downloadWorkerUpdates.Any())
        {
            return;
        }

        // Update every DownloadWorkerTask with the updated progress
        foreach (var downloadWorkerTask in downloadWorkerUpdates)
        {
            var i = DownloadTask.DownloadWorkerTasks.FindIndex(x => x.Id == downloadWorkerTask.Id);
            if (i > -1)
            {
                DownloadTask.DownloadWorkerTasks[i] = downloadWorkerTask;
            }
        }

        DownloadTask.DataReceived = DownloadTask.DownloadWorkerTasks.Sum(x => x.BytesReceived);
        DownloadTask.Percentage = DownloadTask.DownloadWorkerTasks.Average(x => x.Percentage);
        DownloadTask.DownloadSpeed = DownloadTask.DownloadWorkerTasks.Sum(x => x.DownloadSpeed);

        DownloadStatus = DownloadTaskActions.Aggregate(downloadWorkerUpdates.Select(x => x.DownloadStatus).ToList());

        _downloadTaskUpdate.OnNext(DownloadTask);

        if (DownloadStatus == DownloadStatus.DownloadFinished)
        {
            _downloadTaskUpdate.OnCompleted();
            _downloadWorkerLog.OnCompleted();
        }
    }

    private void SetupDownloadLimitWatcher()
    {
        void SetDownloadSpeedLimit(int downloadSpeedLimitInKb)
        {
            foreach (var downloadWorker in _downloadWorkers)
            {
                downloadWorker.SetDownloadSpeedLimit(downloadSpeedLimitInKb / _downloadWorkers.Count);
            }
        }

        SetDownloadSpeedLimit(_serverSettings.GetDownloadSpeedLimit(DownloadTask.PlexServerId));
        _downloadSpeedLimitSubscription = _serverSettings
            .GetDownloadSpeedLimitObservable(DownloadTask.PlexServerId)
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
            .Sample(TimeSpan.FromMilliseconds(400), _timeThreadContext)
            .Subscribe(OnDownloadWorkerTaskUpdate);

        // On download worker log
        _downloadWorkers
            .Select(x => x.DownloadWorkerLog)
            .Merge()
            .Subscribe(downloadWorkerLog => _downloadWorkerLog.OnNext(downloadWorkerLog));
    }

    #endregion
}