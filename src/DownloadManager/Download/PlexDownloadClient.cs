using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using PlexRipper.Application.Common;
using PlexRipper.Application.Common.DTO.DownloadManager;
using PlexRipper.Domain;

namespace PlexRipper.DownloadManager.Download
{
    /// <summary>
    /// The PlexDownloadClient handles a single <see cref="DownloadTask"/> at a time and
    /// manages the <see cref="DownloadWorker"/>s responsible for the multi-threaded downloading.
    /// </summary>
    public class PlexDownloadClient : IDisposable
    {
        #region Fields

        private readonly Subject<DownloadClientUpdate> _downloadClientUpdate = new();

        private readonly Subject<DownloadWorkerLog> _downloadWorkerLog = new();

        private readonly List<DownloadWorker> _downloadWorkers = new();

        private readonly IFileSystemCustom _fileSystemCustom;

        private readonly EventLoopScheduler _timeThreadContext = new();

        private readonly CancellationTokenSource _downloadWorkersToken = new CancellationTokenSource();

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PlexDownloadClient"/> class.
        /// </summary>
        /// <param name="downloadTask">The <see cref="DownloadTask"/> to start executing.</param>
        /// <param name="fileSystemCustom">Used to get the (file)Streams in which to store the download data.</param>
        /// <param name="plexRipperHttpClient"></param>
        /// <param name="downloadSpeedLimit">The download speed limit, 0 is unlimited</param>
        private PlexDownloadClient(
            DownloadTask downloadTask,
            IFileSystemCustom fileSystemCustom,
            IPlexRipperHttpClient plexRipperHttpClient,
            int downloadSpeedLimit = 0)
        {
            _fileSystemCustom = fileSystemCustom;
            DownloadTask = downloadTask;

            foreach (var downloadWorkerTask in downloadTask.DownloadWorkerTasks)
            {
                _downloadWorkers.Add(new DownloadWorker(downloadWorkerTask, fileSystemCustom, plexRipperHttpClient, downloadSpeedLimit));
            }

            SetupSubscriptions();
        }

        public static Result<PlexDownloadClient> Create(DownloadTask downloadTask, IFileSystemCustom fileSystemCustom,
            IPlexRipperHttpClient plexRipperHttpClient, int downloadSpeedLimit = 0)
        {
            // Create workers
            if (downloadTask is null)
            {
                return Result.Fail(new Error("DownloadTask parameter in PlexDownloadClient was null")).LogError();
            }

            if (downloadTask.DownloadWorkerTasks is null || !downloadTask.DownloadWorkerTasks.Any())
            {
                return Result.Fail(new Error("DownloadTask.DownloadWorkerTasks in PlexDownloadClient was null or empty")).LogError();
            }

            var downloadTaskValidResult = downloadTask.IsValid();
            if (downloadTaskValidResult.IsFailed)
            {
                Log.Error($"DownloadTask with id {downloadTask.Id} and name {downloadTask.FileName} failed validation");
                return downloadTaskValidResult.LogError();
            }

            return Result.Ok(new PlexDownloadClient(downloadTask, fileSystemCustom, plexRipperHttpClient, downloadSpeedLimit));
        }

        #endregion

        #region Properties

        public DateTime DownloadStartAt { get; internal set; }

        public DownloadStatus DownloadStatus
        {
            get => DownloadTask.DownloadStatus;
            internal set => DownloadTask.DownloadStatus = value;
        }

        public DownloadTask DownloadTask { get; }

        /// <summary>
        /// The ClientId/DownloadTaskId is always the same id that is assigned to the <see cref="DownloadTask"/>.
        /// </summary>
        public int DownloadTaskId => DownloadTask.Id;

        /// <summary>
        /// In how many parts/segments should the media be downloaded.
        /// </summary>
        public long Parts => _downloadWorkers.Count;

        /// <summary>
        /// Gets the Task that completes when all download workers have finished.
        /// </summary>
        public Task DownloadProcessTask => Task.WhenAll(_downloadWorkers.Select(x => x.DownloadProcessTask));

        #region Observables

        public IObservable<DownloadClientUpdate> DownloadClientUpdate => _downloadClientUpdate.AsObservable();

        public IObservable<DownloadWorkerLog> DownloadWorkerLog => _downloadWorkerLog.AsObservable();

        #endregion

        #endregion

        #region Methods

        #region Private

        /// <summary>
        /// Releases the unmanaged resources used by the HttpClient and optionally disposes of the managed resources.
        /// </summary>
        public void Dispose()
        {
            _downloadWorkerLog?.Dispose();
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
                .Select(x => x.DownloadWorkerUpdate)
                .CombineLatest()
                .Sample(TimeSpan.FromMilliseconds(1000), _timeThreadContext)
                .Subscribe(OnDownloadWorkerUpdate);

            // On download worker log
            _downloadWorkers
                .Select(x => x.DownloadWorkerLog)
                .Merge()
                .Subscribe(OnDownloadWorkerLog);
        }

        /// <summary>
        /// Calls Dispose on all DownloadWorkers and clears the downloadWorkersList.
        /// </summary>
        /// <returns>Is successful.</returns>
        private async Task ClearDownloadWorkers()
        {
            await Task.WhenAll(_downloadWorkers.Select(x => x.DisposeAsync()).ToList());
            _downloadWorkers.Clear();
            Log.Debug($"DownloadWorkers have been disposed for {DownloadTask.DownloadPath}");
        }

        #region Subscriptions

        private void OnDownloadWorkerUpdate(IList<DownloadWorkerUpdate> downloadWorkerUpdates)
        {
            if (_downloadClientUpdate.IsDisposed)
            {
                return;
            }

            var clientStatus = downloadWorkerUpdates.Select(x => x.DownloadStatus).ToList();

            // If there is any error then set client to error state
            if (clientStatus.Any(x => x == DownloadStatus.Error))
            {
                DownloadStatus = DownloadStatus.Error;
            }

            if (clientStatus.Any(x => x == DownloadStatus.Downloading))
            {
                DownloadStatus = DownloadStatus.Downloading;
            }

            if (clientStatus.All(x => x == DownloadStatus.Completed))
            {
                DownloadStatus = DownloadStatus.Completed;
            }

            var downloadClientUpdate = new DownloadClientUpdate(DownloadTask, downloadWorkerUpdates);

            Log.Debug(downloadClientUpdate.ToString());

            _downloadClientUpdate.OnNext(downloadClientUpdate);

            if (DownloadStatus == DownloadStatus.Completed)
            {
                _downloadClientUpdate.OnCompleted();
                _downloadWorkerLog.OnCompleted();
            }
        }

        private void OnDownloadWorkerLog(DownloadWorkerLog downloadWorkerLog)
        {
            _downloadWorkerLog.OnNext(downloadWorkerLog);
        }

        #endregion

        #endregion

        #region Public

        #region Commands

        /// <summary>
        /// Starts the download workers for the <see cref="DownloadTask"/> given during setup.
        /// </summary>
        /// <returns>Is successful.</returns>
        public async Task<Result> Start()
        {
            Log.Debug($"Start downloading {DownloadTask.FileName} from {DownloadTask.DownloadUrl}");
            DownloadStartAt = DateTime.UtcNow;
            try
            {
                foreach (var downloadWorker in _downloadWorkers)
                {
                    var startResult = downloadWorker.Start();
                    if (startResult.IsFailed)
                    {
                        await ClearDownloadWorkers();
                        return startResult.LogError();
                    }
                }
            }
            catch (Exception e)
            {
                await ClearDownloadWorkers();
                return Result.Fail(new ExceptionalError(e)
                        .WithMessage($"Could not download {DownloadTask.FileName} from {DownloadTask.DownloadUrl}"))
                    .LogError();
            }

            return Result.Ok();
        }

        /// <summary>
        /// Immediately stops all and destroys the <see cref="DownloadWorker"/>s, will also removes any temporary files them.
        /// This will also remove any downloaded data.
        /// </summary>
        /// <returns>Is successful.</returns>
        public async Task<Result> Stop()
        {
            _downloadWorkers.AsParallel().ForAll(async downloadWorker =>
            {
                var stopResult = await downloadWorker.Stop();
                if (stopResult.IsFailed)
                {
                    stopResult.WithError(new Error(
                            $"Failed to stop downloadWorkerTask with id: {downloadWorker.Id} in PlexDownloadClient with id: {DownloadTask.Id}"))
                        .LogError();
                }
            });

            await ClearDownloadWorkers();

            _fileSystemCustom.DeleteAllFilesFromDirectory(DownloadTask.DownloadPath);
            _fileSystemCustom.DeleteDirectoryFromFilePath(DownloadTask.DownloadPath);

            return Result.Ok(true);
        }

        public async Task<Result<List<DownloadWorkerTask>>> Pause()
        {
            var downloadWorkerTasks = _downloadWorkers.Select(x => x.DownloadWorkerTask).ToList();

            await ClearDownloadWorkers();

            if (DownloadStatus == DownloadStatus.Downloading)
            {
                _downloadWorkers.AsParallel().ForAll(async downloadWorker =>
                {
                    var pauseResult = await downloadWorker.Stop();
                    if (pauseResult.IsFailed)
                    {
                        pauseResult.WithError(new Error(
                                $"Failed to pause downloadWorkerTask with id: {downloadWorker.Id} in PlexDownloadClient with id: {DownloadTask.Id}"))
                            .LogError();
                    }
                });
            }

            return Result.Ok();
        }

        public void SetDownloadSpeedLimit(int downloadSpeedLimitInKb = 0)
        {
            _downloadWorkers.ForEach(x => x.SetDownloadSpeedLimit(downloadSpeedLimitInKb /  _downloadWorkers.Count));
        }

        #endregion

        #endregion

        #endregion
    }
}