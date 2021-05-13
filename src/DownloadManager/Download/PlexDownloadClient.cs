using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using FluentResults;
using MediatR;
using PlexRipper.Application.Common;
using PlexRipper.Application.Common.DTO.DownloadManager;
using PlexRipper.Application.PlexDownloads;
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

        private readonly IMediator _mediator;

        private readonly IFileSystemCustom _fileSystemCustom;

        private readonly Func<DownloadWorkerTask, DownloadWorker> _downloadWorkerFactory;

        private readonly EventLoopScheduler _timeThreadContext = new();

        private bool _isSetup;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PlexDownloadClient"/> class.
        /// </summary>
        /// <param name="downloadTask">The <see cref="DownloadTask"/> to start executing.</param>
        /// <param name="mediator"></param>
        /// <param name="fileSystemCustom">Used to get fileStreams in which to store the download data.</param>
        /// <param name="httpClientFactory"></param>
        /// <param name="downloadWorkerFactory"></param>
        public PlexDownloadClient(DownloadTask downloadTask, IMediator mediator, IFileSystemCustom fileSystemCustom,
            Func<DownloadWorkerTask, DownloadWorker> downloadWorkerFactory)
        {
            _mediator = mediator;
            _fileSystemCustom = fileSystemCustom;
            _downloadWorkerFactory = downloadWorkerFactory;
            DownloadTask = downloadTask;
            DownloadStatus = downloadTask.DownloadStatus;
        }

        #endregion

        #region Properties

        public DateTime DownloadStartAt { get; internal set; }

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

        /// <summary>
        /// In how many parts/segments should the media be downloaded.
        /// </summary>
        public long Parts { get; set; } = 1;

        public long TotalBytesToReceive => DownloadTask.DataTotal;

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
        /// <param name="disposing">Is currently disposing.</param>
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
        public async Task<Result<bool>> Start()
        {
            if (!_isSetup)
            {
                return Result.Fail(new Error("This plex download client has not been setup, run SetupAsync() first")).LogError();
            }

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
        /// Setup this PlexDownloadClient to get ready for downloading.
        /// </summary>
        /// <param name="downloadWorkerTasks">Optional: If the <see cref="DownloadWorkerTask"/>s are already made then use those,
        /// otherwise they will be created.</param>
        /// <returns>Is successful.</returns>
        public async Task<Result<bool>> SetupAsync()
        {
            var downloadWorkerTasks = DownloadTask.DownloadWorkerTasks;

            if (downloadWorkerTasks is null || !downloadWorkerTasks.Any())
            {
                // Create download worker tasks/segments/ranges
                var partSize = TotalBytesToReceive / Parts;
                var remainder = TotalBytesToReceive - partSize * Parts;
                downloadWorkerTasks = new List<DownloadWorkerTask>();
                for (int i = 0; i < Parts; i++)
                {
                    long start = partSize * i;
                    long end = start + partSize;
                    if (i == Parts - 1 && remainder > 0)
                    {
                        // Add the remainder to the last download range
                        end += remainder;
                    }

                    downloadWorkerTasks.Add(new DownloadWorkerTask(DownloadTask, i + 1, start, end));
                }

                // Verify bytes have been correctly divided
                var totalBytesInWorkers = downloadWorkerTasks.Sum(x => x.BytesRangeSize);
                if (totalBytesInWorkers != TotalBytesToReceive)
                {
                    Log.Error($"The bytes were incorrectly divided, expected {TotalBytesToReceive} but the sum was " +
                              $"{totalBytesInWorkers} with a difference of {TotalBytesToReceive - totalBytesInWorkers}");
                }

                // Send downloadWorkerTasks to the database and retrieve entries
                var result = await _mediator.Send(new AddDownloadWorkerTasksCommand(downloadWorkerTasks));
                if (result.IsFailed)
                {
                    return result.ToResult().LogError();
                }
            }

            var createResult = await CreateDownloadWorkers(DownloadTask.Id);
            if (createResult.IsFailed)
            {
                return createResult;
            }

            SetupSubscriptions();
            _isSetup = true;
            return Result.Ok(true);
        }

        private async Task<Result<bool>> CreateDownloadWorkers(int downloadTaskId)
        {
            var downloadTask = await _mediator.Send(new GetDownloadTaskByIdQuery(downloadTaskId, true, true));
            if (downloadTask.IsFailed)
            {
                return downloadTask.ToResult();
            }

            if (!downloadTask.Value.DownloadWorkerTasks.Any())
            {
                return Result.Fail($"Could not find any download worker tasks attached to download task {downloadTaskId}").LogError();
            }

            // Update the Download Task set in this client
            DownloadTask = downloadTask.Value;

            // Create workers
            foreach (var downloadWorkerTask in downloadTask.Value.DownloadWorkerTasks)
            {
                _downloadWorkers.Add(_downloadWorkerFactory(downloadWorkerTask));
            }

            return Result.Ok(true);
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

        public async Task<Result> Pause()
        {
            if (DownloadStatus == DownloadStatus.Downloading)
            {
                _downloadWorkers.AsParallel().ForAll(async downloadWorker =>
                {
                    var pauseResult = await downloadWorker.Pause();
                    if (pauseResult.IsFailed)
                    {
                        pauseResult.WithError(new Error(
                                $"Failed to pause downloadWorkerTask with id: {downloadWorker.Id} in PlexDownloadClient with id: {DownloadTask.Id}"))
                            .LogError();
                    }
                });

                await ClearDownloadWorkers();
                return Result.Ok();
            }

            return Result.Ok();
        }

        #endregion

        #endregion

        #endregion
    }
}