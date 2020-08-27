using FluentResults;
using PlexRipper.Application.Common.Interfaces.FileSystem;
using PlexRipper.Domain;
using PlexRipper.Domain.Entities;
using PlexRipper.DownloadManager.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PlexRipper.Application.Common.Interfaces.DownloadManager;
using PlexRipper.Domain.Common;
using PlexRipper.Domain.Enums;

namespace PlexRipper.DownloadManager.Download
{
    public class PlexDownloadClient : HttpClient
    {


        #region Fields

        private readonly CancellationTokenSource _cancellationToken = new CancellationTokenSource();
        private readonly IFileSystem _fileSystem;
        private readonly Progress<long> _progress = new Progress<long>();
        private List<DownloadWorker> _downloadWorkers = new List<DownloadWorker>();
        private readonly Subject<DownloadProgress> _downloadProgressChanged = new Subject<DownloadProgress>();
        private readonly Subject<DownloadComplete> _downloadFileCompleted = new Subject<DownloadComplete>();
        private readonly Subject<DownloadStatusChanged> _statusChanged = new Subject<DownloadStatusChanged>();

        #endregion Fields

        #region Constructors

        public PlexDownloadClient(DownloadTask downloadTask, IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
            DownloadTask = downloadTask;
            DownloadStatus = downloadTask.DownloadStatus;
            // AddHeaders();

            // Setup progress changed event
            _progress.ProgressChanged += (sender, l) =>
            {
                BytesReceived = l;
                BytesRemaining = TotalBytesToReceive - BytesReceived;
            };
        }

        #endregion Constructors

        #region Properties

        public string PlexServerAuthToken { get; set; }

        /// <summary>
        /// The ClientId/DownloadTaskId is always the same id that is assigned to the <see cref="DownloadTask"/>
        /// </summary>
        public int DownloadTaskId => DownloadTask.Id;

        // Size of downloaded data which was written to the local file
        public long DownloadedSize { get; set; }

        public string DownloadPath => _fileSystem.ToAbsolutePath(DownloadTask.DestinationFolder?.Directory);

        public DateTime DownloadStartAt { get; internal set; }

        public DownloadTask DownloadTask { get; internal set; }

        public TimeSpan ElapsedTime => DateTime.UtcNow.Subtract(DownloadStartAt);

        public long BytesReceived { get; internal set; }
        public long BytesRemaining { get; internal set; }

        public long TotalBytesToReceive { get; internal set; }
        public DownloadStatus DownloadStatus { get; internal set; }

        public string DownloadUrl => $"{DownloadTask.DownloadUrl}?download=1&X-Plex-Token={PlexServerAuthToken}";

        /// <summary>
        /// In how many parts/segments should the media be downloaded.
        /// </summary>
        public long Parts { get; set; } = 1;

        #endregion Properties

        #region Methods

        private void SetDownloadStatus(DownloadStatus downloadStatus)
        {
            DownloadStatus = downloadStatus;
            _statusChanged.OnNext(new DownloadStatusChanged(DownloadTaskId, downloadStatus));
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
                var downloadRange = new DownloadRange(i + 1, DownloadUrl, start, end, DownloadTask.FileName, DownloadTask.DownloadPath);
                var downloadWorker = new DownloadWorker(downloadRange, _fileSystem, _cancellationToken.Token);
                _downloadWorkers.Add(downloadWorker);
            }
            _downloadWorkers
                .Select(x => x.DownloadWorkerProgress)
                .CombineLatest()
                .DistinctUntilChanged()
                // .Sample(TimeSpan.FromMilliseconds(1000))
                .Subscribe(OnDownloadProgressChanged);
            _downloadWorkers
                .Select(x => x.DownloadStatusChanged)
                .CombineLatest()
                .DistinctUntilChanged()
                // .Sample(TimeSpan.FromMilliseconds(1000))
                .Subscribe(OnDownloadStatusChanged);
            _downloadWorkers
                .Select(x => x.DownloadWorkerComplete.DistinctUntilChanged())
                .CombineLatest()
                .Subscribe(OnDownloadComplete);
        }

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
            //Log.Debug(builder.ToString());
            _downloadProgressChanged.OnNext(downloadProgress);
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
            if (completeList.Count != Parts)
            {
                return;
            }
            var orderedList = completeList.ToList().OrderBy(x => x.Id).ToList();
            StringBuilder builder = new StringBuilder();
            foreach (var progress in orderedList)
            {
                builder.Append($"({progress.Id} - {progress.FileName} download completed!) + ");
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
                DownloadWorkerCompletes = orderedList
            };
            Log.Debug($"Download of {DownloadTask.FileName} finished!");
            SetDownloadStatus(DownloadStatus.Completed);
            ClearDownloadWorkers();
            _downloadFileCompleted.OnNext(downloadComplete);
        }

        #endregion

        #endregion Methods

        #region Observables

        public IObservable<DownloadProgress> DownloadProgressChanged => _downloadProgressChanged.AsObservable();

        public IObservable<DownloadComplete> DownloadFileCompleted => _downloadFileCompleted.AsObservable();

        public IObservable<DownloadStatusChanged> DownloadStatusChanged => _statusChanged.AsObservable();

        #endregion

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
    }
}