using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using PlexRipper.Application.Common;
using PlexRipper.Domain;
using PlexRipper.Domain.Entities;
using PlexRipper.Domain.Enums;
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

        private readonly CancellationTokenSource _cancellationToken = new CancellationTokenSource();
        private readonly Subject<DownloadComplete> _downloadFileCompleted = new Subject<DownloadComplete>();
        private readonly Subject<DownloadProgress> _downloadProgressChanged = new Subject<DownloadProgress>();

        private readonly List<DownloadWorker> _downloadWorkers = new List<DownloadWorker>();
        private readonly IFileSystem _fileSystem;
        private readonly Subject<DownloadStatusChanged> _statusChanged = new Subject<DownloadStatusChanged>();

        private IDisposable _workerProgressSubscription;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PlexDownloadClient"/> class.
        /// </summary>
        /// <param name="downloadTask">The <see cref="DownloadTask"/> to start executing.</param>
        /// <param name="fileSystem">Used to get fileStreams in which to store the download data.</param>
        public PlexDownloadClient(DownloadTask downloadTask, IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
            DownloadTask = downloadTask;
            DownloadStatus = downloadTask.DownloadStatus;
        }

        #endregion

        #region Properties

        public string DownloadPath => _fileSystem.ToAbsolutePath(DownloadTask.DestinationFolder?.Directory);

        public DateTime DownloadStartAt { get; internal set; }

        public DownloadStatus DownloadStatus { get; internal set; }

        public DownloadTask DownloadTask { get; internal set; }

        /// <summary>
        /// The ClientId/DownloadTaskId is always the same id that is assigned to the <see cref="DownloadTask"/>
        /// </summary>
        public int DownloadTaskId => DownloadTask.Id;

        public string DownloadUrl => $"{DownloadTask.DownloadUrl}?download=1&X-Plex-Token={PlexServerAuthToken}";

        public TimeSpan ElapsedTime => DateTime.UtcNow.Subtract(DownloadStartAt);

        /// <summary>
        /// In how many parts/segments should the media be downloaded.
        /// </summary>
        public long Parts { get; set; } = 1;

        public string PlexServerAuthToken { get; set; }

        public long TotalBytesToReceive { get; internal set; }

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
                _cancellationToken?.Dispose();
                _downloadProgressChanged?.Dispose();
                _downloadFileCompleted?.Dispose();
                _statusChanged?.Dispose();
            }

            base.Dispose(disposing);
        }

        private void CreateDownloadWorkers()
        {
            // Create download segments/ranges
            var partSize = TotalBytesToReceive / Parts;
            var remainder = TotalBytesToReceive - (partSize * Parts);
            for (int i = 0; i < Parts; i++)
            {
                long start = partSize * i;
                long end = start + partSize;
                if (i == Parts - 1 && remainder > 0)
                {
                    // Add the remainder to the last download range
                    end += remainder;
                }

                var downloadRange = new DownloadRange(
                    i + 1,
                    DownloadUrl,
                    start,
                    end,
                    DownloadTask.FileName,
                    DownloadTask.DownloadPath);

                var downloadWorker = new DownloadWorker(downloadRange, _fileSystem, _cancellationToken.Token);
                _downloadWorkers.Add(downloadWorker);
            }

            // Verify bytes have been correctly divided
            var totalBytesInWorkers = _downloadWorkers.Sum(x => x.DownloadRange.RangeSize);
            if (totalBytesInWorkers != TotalBytesToReceive)
            {
                Log.Error($"The bytes were incorrectly divided, expected {TotalBytesToReceive} but the sum was " +
                          $"{totalBytesInWorkers} with a difference of {TotalBytesToReceive - totalBytesInWorkers}");
            }

            SetupSubscriptions();
        }

        private void SetDownloadStatus(DownloadStatus downloadStatus)
        {
            DownloadStatus = downloadStatus;
            _statusChanged.OnNext(new DownloadStatusChanged(DownloadTaskId, downloadStatus));
        }

        private void SetupSubscriptions()
        {
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
                .Sample(TimeSpan.FromMilliseconds(1000))
                .Subscribe(OnDownloadProgressChanged);

            // On download change
            _downloadWorkers
                .Select(x => x.DownloadStatusChanged)
                .CombineLatest()
                .TakeUntil(downloadCompleteStream)
                .Subscribe(OnDownloadStatusChanged);

            // On download complete
            downloadCompleteStream
                .Subscribe(OnDownloadComplete);
        }

        #region Subscriptions

        private void OnDownloadProgressChanged(IList<IDownloadWorkerProgress> progressList)
        {
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
            Log.Debug(builder.ToString());
            _downloadProgressChanged.OnNext(downloadProgress);

            if (progressList.All(x => x.IsCompleted)) { }
        }

        private void OnDownloadStatusChanged(IList<DownloadStatusChanged> statuses)
        {
            foreach (var downloadStatusChanged in statuses)
            {
                if (downloadStatusChanged.Status == DownloadStatus.Error)
                {
                    // TODO Add error handling and functionality to communicate to the front-end
                    SetDownloadStatus(DownloadStatus.Error);
                    break;
                }

                // Check recursively if all _downloadWorkers have the same download status
                if (_downloadWorkers.All(x => x.Status == downloadStatusChanged.Status))
                {
                    SetDownloadStatus(downloadStatusChanged.Status);
                    break;
                }
            }
        }

        private void OnDownloadComplete(IList<DownloadWorkerComplete> completeList)
        {
            _workerProgressSubscription.Dispose();
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
            var downloadComplete = new DownloadComplete(DownloadTaskId)
            {
                DestinationPath = DownloadPath,
                FileName = DownloadTask.FileName,
                DownloadWorkerCompletes = orderedList,
                DataReceived = completeList.Sum(x => x.BytesReceived),
                DataTotal = TotalBytesToReceive,
            };
            Log.Debug($"Download of {DownloadTask.FileName} finished!");
            ClearDownloadWorkers();
            SetDownloadStatus(DownloadStatus.Completed);
            _downloadFileCompleted.OnNext(downloadComplete);
        }

        #endregion

        #endregion

        #region Public

        #region Commands

        /// <summary>
        /// Start the download of the DownloadTask passed during the construction.
        /// </summary>
        /// <returns></returns>
        public async Task<Result<bool>> StartAsync()
        {
            Log.Debug($"Start downloading from {DownloadUrl}");
            try
            {
                SetDownloadStatus(DownloadStatus.Starting);

                // Source: https://stackoverflow.com/a/48190014/8205497
                var response = await GetAsync(DownloadUrl, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();
                TotalBytesToReceive = response.Content.Headers.ContentLength ?? -1L;
                if (TotalBytesToReceive <= 0)
                {
                    SetDownloadStatus(DownloadStatus.Error);
                    return Result.Fail("File size could not be determined of the media that will be downloaded");
                }

                CreateDownloadWorkers();
                StartDownloadWorkers();
                return Result.Ok(true);
            }
            catch (Exception e)
            {
                var msg = $"Could not download {DownloadTask.FileName} from {DownloadTask.DownloadUrl}";
                Log.Error(e, msg);
                var result = Result.Fail(new ExceptionalError(e));
                result.Errors.Add(new Error(msg));
                Stop();
                return result;
            }
        }

        public Result<bool> ClearDownloadWorkers()
        {
            foreach (var downloadWorker in _downloadWorkers)
            {
                downloadWorker.Dispose();
            }

            _downloadWorkers.Clear();
            return Result.Ok(true);
        }

        /// <summary>
        /// Immediately stops all <see cref="DownloadWorker"/>s and destroys them.
        /// This will also remove any downloaded data.
        /// </summary>
        /// <returns>Is successful.</returns>
        public Result<bool> Stop()
        {
            try
            {
                _cancellationToken?.Cancel();
                foreach (var downloadWorker in _downloadWorkers)
                {
                    downloadWorker.Stop();
                }
            }
            catch (Exception e)
            {
                return Result.Fail($"Failed to cancel downloadTask with id: {DownloadTaskId}").LogError(e);
            }

            ClearDownloadWorkers();
            return Result.Ok(true);
        }

        public async Task<Result<bool>> Restart()
        {
            if (DownloadStatus == DownloadStatus.Downloading)
            {
                Stop();
            }

            await StartAsync();
            return Result.Ok(true);
        }


        private void StartDownloadWorkers()
        {
            DownloadStartAt = DateTime.UtcNow;
            SetDownloadStatus(DownloadStatus.Downloading);
            Task.WhenAll(_downloadWorkers.Select(x => x.Start().ValueOrDefault).ToList());
        }

        #endregion

        #region Observables

        public IObservable<DownloadProgress> DownloadProgressChanged => _downloadProgressChanged.AsObservable();

        public IObservable<DownloadComplete> DownloadFileCompleted => _downloadFileCompleted.AsObservable();

        public IObservable<DownloadStatusChanged> DownloadStatusChanged => _statusChanged.AsObservable();

        public IObservable<DownloadStatusChanged> DownloadMetaChanged => _statusChanged.AsObservable();

        #endregion

        #endregion

        #endregion
    }
}