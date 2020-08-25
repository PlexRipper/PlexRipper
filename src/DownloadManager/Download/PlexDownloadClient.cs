using FluentResults;
using PlexRipper.Application.Common.Interfaces.FileSystem;
using PlexRipper.Domain;
using PlexRipper.Domain.Entities;
using PlexRipper.Domain.Types;
using PlexRipper.DownloadManager.Common;
using PlexRipper.PlexApi.Api;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        private Task _copyTask;
        private Stream _fileStream;
        private Stream _responseStream;
        private System.Timers.Timer _progressTimer = new System.Timers.Timer(1000) {AutoReset = true};
        private List<DownloadWorker> _downloadWorkers = new List<DownloadWorker>();

        #endregion Fields

        #region Constructors

        public PlexDownloadClient(DownloadTask downloadTask, IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
            DownloadTask = downloadTask;
            AddHeaders();

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

        public string DownloadPath => _fileSystem.ToAbsolutePath(DownloadTask.FolderPath?.Directory);

        public long DownloadSpeed { get; internal set; }

        public DateTime DownloadStartAt { get; internal set; }

        public DownloadTask DownloadTask { get; internal set; }

        public TimeSpan ElapsedTime => DateTime.UtcNow.Subtract(DownloadStartAt);

        public long BytesReceived { get; internal set; }
        public long BytesRemaining { get; internal set; }

        public long TotalBytesToReceive { get; internal set; }
        public DownloadStatus DownloadStatus => DownloadTask.DownloadStatus;

        public string DownloadUrl => $"{DownloadTask.DownloadUrl}?download=1&X-Plex-Token={PlexServerAuthToken}";

        /// <summary>
        /// In how many parts/segments should the media be downloaded.
        /// </summary>
        public long Parts { get; set; } = 1;

        #endregion Properties

        #region Methods

        private void AddHeaders()
        {
            foreach ((string key, string value) in PlexHeaderData.GetBasicHeaders)
            {
                this.DefaultRequestHeaders.Add(key, value);
            }
        }

        private void SetDownloadStatus(DownloadStatus downloadStatus)
        {
            DownloadTask.DownloadStatus = downloadStatus;
        }



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

                await CreateDownloadWorkers();

                // _responseStream = await response.Content.ReadAsStreamAsync();
                // var result = _fileSystem.SaveFile(DownloadPath, DownloadTask.FileName, TotalBytesToReceive);
                // if (result.IsFailed)
                // {
                //     SetDownloadStatus(DownloadStatus.Error);
                //     return result.ToResult();
                // }
                // _fileStream = result.Value;
                // if (_fileStream == null)
                // {
                //     SetDownloadStatus(DownloadStatus.Error);
                //     return Result.Fail($"The file stream was null with destination {DownloadPath}");
                // }
                //
                // // Set Timings

                // _progressTimer.Start();
                //
                // SetDownloadStatus(DownloadStatus.Downloading);
                //
                // _copyTask = _responseStream
                //     .CopyToAsync(_fileStream, _progress, 81920, _cancellationToken.Token)
                //     .ContinueWith(task =>
                //     {
                //         if (task.IsCanceled) return;
                //         _responseStream.Dispose();
                //         _fileStream.Dispose();
                //     });

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

        public Result<bool> Stop()
        {
            // TODO using exception like this might be dangerous
            try
            {
                _progressTimer.Stop();
                _cancellationToken?.Cancel();
                _responseStream?.Dispose();
                _fileStream?.Dispose();

                //  _copyTask.
            }
            catch (Exception e)
            {
                return Result.Fail($"Failed to cancel downloadTask with id: {DownloadTaskId}").LogError(e);
            }
            return Result.Ok(true);
        }

        private List<DownloadRange> CreateDownloadRanges()
        {
            // Divide into equal parts
            var partSize = TotalBytesToReceive / Parts;
            var rangeList = new List<DownloadRange>((int) Parts);
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
                rangeList.Add(new DownloadRange(DownloadUrl, start, end));
            }

            return rangeList;
        }

        private async Task CreateDownloadWorkers()
        {
            var list = CreateDownloadRanges();
            var i = 1;
            foreach (var downloadRange in list)
            {
                var downloadWorker = new DownloadWorker(i++, DownloadTask, downloadRange, _fileSystem);
                _downloadWorkers.Add(downloadWorker);
            }

            _downloadWorkers
                .Select(x => x.DownloadWorkerProgressSubject)
                .Merge()
                .DistinctUntilChanged()
                .Buffer(_downloadWorkers.Count)

                //.Throttle(TimeSpan.FromSeconds(1), Scheduler.CurrentThread)
                .Subscribe(OnDownloadProgressChanged);

            await StartDownloadWorkers();
        }

        private void OnDownloadProgressChanged(IList<DownloadWorkerProgress> progressList)
        {
            var listOrdered = progressList.ToList().OrderBy(x => x.Id).ToList();
            StringBuilder builder = new StringBuilder();
            foreach (var progress in listOrdered)
            {
                builder.Append($"({progress.Id} - {progress.Percentage}) + ");
            }

            Log.Debug(builder.ToString());

            var downloadProgress = new DownloadProgress(listOrdered)
            {
                Id = DownloadTaskId,
                Status = DownloadTask.DownloadStatus.ToString(),
                DataTotal = TotalBytesToReceive,
                DownloadSpeed = DownloadSpeed,
                TimeRemaining = (int) Math.Floor(BytesRemaining / (double) DownloadSpeed),
            };

            DownloadProgressChanged.OnNext(downloadProgress);

            if (listOrdered.All(x => x.IsCompleted))
            {
                var downloadComplete = new DownloadComplete(DownloadTaskId)
                {
                    ChuckPaths = _downloadWorkers.Select(x => x.FilePath).ToList(),
                    DestinationPath = DownloadPath,
                    FileName = DownloadTask.FileName
                };

                Log.Debug($"Download of {DownloadTask.FileName} finished!");

                DownloadFileCompleted.OnNext(downloadComplete);
            }
        }

        private async Task StartDownloadWorkers()
        {
            DownloadStartAt = DateTime.UtcNow;
            await Task.WhenAll(_downloadWorkers.Select(x => x.Start().ValueOrDefault).ToList());
        }

        #endregion Methods

        #region Events

        public Subject<DownloadProgress> DownloadProgressChanged { get; } = new Subject<DownloadProgress>();


        public Subject<DownloadComplete> DownloadFileCompleted { get; } = new Subject<DownloadComplete>();

        public event EventHandler<DownloadStatus> DownloadStatusChanged;

        #endregion Events
    }
}