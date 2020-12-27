using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using FluentResults;
using MediatR;
using PlexRipper.Application.Common;
using PlexRipper.Application.PlexDownloads;
using PlexRipper.Domain;
using PlexRipper.DownloadManager.Common;

namespace PlexRipper.DownloadManager.Download
{
    /// <summary>
    /// The PlexDownloadClient handles a single <see cref="DownloadTask"/> at a time and
    /// manages the <see cref="DownloadWorker"/>s responsible for the multi-threaded downloading.
    /// </summary>
    public class PlexDownloadClient : HttpClient
    {
        #region Fields

        private readonly Subject<DownloadComplete> _downloadFileCompleted = new Subject<DownloadComplete>();

        private readonly Subject<DownloadProgress> _downloadProgressChanged = new Subject<DownloadProgress>();

        private readonly Subject<DownloadStatusChanged> _statusChanged = new Subject<DownloadStatusChanged>();

        private readonly Subject<IList<DownloadWorkerTask>> _downloadWorkerTaskChanged = new Subject<IList<DownloadWorkerTask>>();

        private readonly List<DownloadWorker> _downloadWorkers = new List<DownloadWorker>();

        private readonly IMediator _mediator;

        private readonly IFileSystem _fileSystem;

        private readonly EventLoopScheduler _timeThreadContext = new EventLoopScheduler();

        private IDisposable _workerProgressSubscription;

        private bool _isSetup;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PlexDownloadClient"/> class.
        /// </summary>
        /// <param name="downloadTask">The <see cref="DownloadTask"/> to start executing.</param>
        /// <param name="mediator"></param>
        /// <param name="fileSystem">Used to get fileStreams in which to store the download data.</param>
        public PlexDownloadClient(DownloadTask downloadTask, IMediator mediator, IFileSystem fileSystem)
        {
            _mediator = mediator;
            _fileSystem = fileSystem;
            DownloadTask = downloadTask;
            DownloadStatus = downloadTask.DownloadStatus;
        }

        #endregion

        #region Properties

        public string DownloadPath => _fileSystem.ToAbsolutePath(DownloadTask.DestinationFolder?.DirectoryPath);

        public DateTime DownloadStartAt { get; internal set; }

        public DownloadStatus DownloadStatus { get; internal set; }

        public DownloadTask DownloadTask { get; internal set; }

        /// <summary>
        /// The ClientId/DownloadTaskId is always the same id that is assigned to the <see cref="DownloadTask"/>.
        /// </summary>
        public int DownloadTaskId => DownloadTask.Id;

        public TimeSpan ElapsedTime => DateTime.UtcNow.Subtract(DownloadStartAt);

        /// <summary>
        /// In how many parts/segments should the media be downloaded.
        /// </summary>
        public long Parts { get; set; } = 1;

        public long TotalBytesToReceive => DownloadTask.DataTotal;

        #region Observables

        public IObservable<DownloadProgress> DownloadProgressChanged => _downloadProgressChanged.AsObservable();

        public IObservable<DownloadComplete> DownloadFileCompleted => _downloadFileCompleted.AsObservable();

        public IObservable<DownloadStatusChanged> DownloadStatusChanged => _statusChanged.AsObservable();

        public IObservable<IList<DownloadWorkerTask>> DownloadWorkerTaskChanged => _downloadWorkerTaskChanged.AsObservable();

        #endregion

        #endregion

        #region Methods

        #region Private

        /// <summary>
        /// Releases the unmanaged resources used by the HttpClient and optionally disposes of the managed resources.
        /// </summary>
        /// <param name="disposing">Is currently disposing.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _downloadProgressChanged?.Dispose();
                _downloadFileCompleted?.Dispose();
                _downloadWorkerTaskChanged?.Dispose();
                _statusChanged?.Dispose();
            }

            base.Dispose(disposing);
        }

        private void SetDownloadStatus(DownloadStatus downloadStatus, Result errorResult = null)
        {
            DownloadStatus = downloadStatus;
            DownloadTask.DownloadStatus = downloadStatus;

            // TODO Find a way to handle Download Worker errors

            _statusChanged.OnNext(new DownloadStatusChanged(DownloadTaskId, downloadStatus));
        }

        private void SetupSubscriptions()
        {
            if (!_downloadWorkers.Any())
            {
                Log.Warning("No download workers have been made yet, cannot setup subscriptions.");
                return;
            }

            var downloadCompleteStream = _downloadWorkers
                .Select(x => x.DownloadWorkerComplete)
                .Merge()
                .Buffer(_downloadWorkers.Count)
                .Take(1);

            // On download progress
            _workerProgressSubscription = _downloadWorkers
                .Select(x => x.DownloadWorkerProgress)
                .CombineLatest()
                .TakeUntil(downloadCompleteStream)
                .Sample(TimeSpan.FromMilliseconds(1000), _timeThreadContext)
                .Subscribe(OnDownloadProgressChanged);

            // On download worker error
            _downloadWorkers
                .Select(x => x.DownloadWorkerError)
                .Merge()
                .TakeUntil(downloadCompleteStream)
                .Subscribe(OnDownloadWorkerError);

            // On download status change
            _downloadWorkers
                .Select(x => x.DownloadWorkerTaskChanged)
                .CombineLatest()
                .TakeUntil(downloadCompleteStream)
                .Subscribe(OnDownloadWorkerTaskChange);

            // On download complete
            // Async function in subscription: https://stackoverflow.com/a/30030553/8205497
            downloadCompleteStream
                .Select(l => Observable.FromAsync(() => OnDownloadComplete(l)))
                .Concat()
                .Subscribe();
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

        private void OnDownloadProgressChanged(IList<IDownloadWorkerProgress> progressList)
        {
            if (_downloadProgressChanged.IsDisposed)
            {
                return;
            }

            var orderedList = progressList.ToList().OrderBy(x => x.Id).ToList();
            StringBuilder builder = new StringBuilder();
            foreach (var progress in orderedList)
            {
                builder.Append($"({progress.Id} - {progress.Percentage} {progress.DownloadSpeedFormatted}) + ");
            }

            // Remove the last " + "
            if (builder.Length > 3)
            {
                builder.Length -= 3;
            }

            var downloadProgress = new DownloadProgress(orderedList)
            {
                Id = DownloadTaskId,
                DataTotal = TotalBytesToReceive,
            };
            builder.Append($" = ({downloadProgress.DownloadSpeedFormatted} - {downloadProgress.TimeRemaining})");
            Log.Verbose(builder.ToString());

            _downloadProgressChanged.OnNext(downloadProgress);
        }

        private void OnDownloadWorkerError(Result errorResult)
        {
            SetDownloadStatus(DownloadStatus.Error, errorResult);
        }

        private void OnDownloadWorkerTaskChange(IList<DownloadWorkerTask> taskList)
        {
            _downloadWorkerTaskChanged.OnNext(taskList);
        }

        private async Task OnDownloadComplete(IList<DownloadWorkerComplete> completeList)
        {
            _timeThreadContext.Dispose();

            var orderedList = completeList.ToList().OrderBy(x => x.Id).ToList();
            StringBuilder builder = new StringBuilder();
            foreach (var progress in orderedList)
            {
                builder.Append($"({progress.Id} - {progress.FileName} download completed!) + ");
                if (!progress.ReceivedAllBytes)
                {
                    var msg = $"Did not receive the correct number of bytes for download worker {progress.Id}.";
                    msg +=
                        $" Received {progress.BytesReceived} and not {progress.BytesReceivedGoal} with a difference of {progress.BytesReceivedGoal - progress.BytesReceived}";
                    Log.Error(msg);
                }
            }

            // Remove the last " + "
            if (builder.Length > 3)
            {
                builder.Length -= 3;
            }

            Log.Debug(builder.ToString());
            var downloadComplete = new DownloadComplete(DownloadTask)
            {
                DestinationPath = DownloadTask.DestinationPath,
                DownloadWorkerCompletes = orderedList,
                DataReceived = completeList.Sum(x => x.BytesReceived),
                DataTotal = TotalBytesToReceive,
            };
            Log.Debug($"Download of {DownloadTask.FileName} finished!");

            await ClearDownloadWorkers();
            SetDownloadStatus(DownloadStatus.Completed);
            _downloadFileCompleted.OnNext(downloadComplete);
        }

        #endregion

        #endregion

        #region Public

        #region Commands

        /// <summary>
        /// Starts the download workers for the <see cref="DownloadTask"/> given during setup.
        /// </summary>
        /// <returns>Is successful.</returns>
        public Result Start()
        {
            if (!_isSetup)
            {
                return Result.Fail(new Error("This plex download client has not been setup, run SetupAsync() first"));
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
                        ClearDownloadWorkers();
                        return startResult.LogError();
                    }
                }

                SetDownloadStatus(DownloadStatus.Downloading);
            }
            catch (Exception e)
            {
                ClearDownloadWorkers();
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
        public async Task<Result<bool>> SetupAsync(List<DownloadWorkerTask> downloadWorkerTasks = null)
        {
            if (downloadWorkerTasks == null || !downloadWorkerTasks.Any())
            {
                if (TotalBytesToReceive <= 0)
                {
                    SetDownloadStatus(DownloadStatus.Error);
                    return Result.Fail("File size could not be determined of the media that will be downloaded");
                }

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

                    downloadWorkerTasks.Add(new DownloadWorkerTask(DownloadTask)
                    {
                        PartIndex = i + 1,
                        Url = DownloadTask.DownloadUrl,
                        StartByte = start,
                        EndByte = end,
                    });
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
                    return result.ToResult();
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
                _downloadWorkers.Add(new DownloadWorker(downloadWorkerTask, _fileSystem));
            }

            return Result.Ok(true);
        }

        /// <summary>
        /// Immediately stops all and destroys the <see cref="DownloadWorker"/>s, will also removes any temporary files them.
        /// This will also remove any downloaded data.
        /// </summary>
        /// <returns>Is successful.</returns>
        public Result<bool> Stop()
        {
            SetDownloadStatus(DownloadStatus.Stopping);

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

            ClearDownloadWorkers();

            _fileSystem.DeleteAllFilesFromDirectory(DownloadTask.DownloadPath);
            _fileSystem.DeleteDirectoryFromFilePath(DownloadTask.DownloadPath);

            SetDownloadStatus(DownloadStatus.Stopped);
            return Result.Ok(true);
        }

        public Result<bool> Pause()
        {
            if (DownloadStatus == DownloadStatus.Downloading)
            {
                SetDownloadStatus(DownloadStatus.Pausing);

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

                ClearDownloadWorkers();
                SetDownloadStatus(DownloadStatus.Paused);
                return Result.Ok(true);
            }

            return Result.Ok(false);
        }

        #endregion

        #endregion

        #endregion
    }
}