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

            // Give an download progress update based on the timer
            _progressTimer.Elapsed += (sender, args) =>
            {
                // Ensure the same value is used
                long bytesReceived = BytesReceived;
                CalculateDownloadSpeed(bytesReceived);
                OnDownloadProgressChanged(bytesReceived);
            };
        }

        #endregion Constructors

        #region Properties

        public string PlexServerAuthToken { get; set; }

        /// <summary>
        /// The ClientId is always the same id that is assigned to the <see cref="DownloadTask"/>
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

        private void CalculateDownloadSpeed(long bytesReceived)
        {
            DownloadSpeed = Convert.ToInt64(Math.Round(bytesReceived / ElapsedTime.TotalSeconds, 2));
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

                var list = CreateDownloadRanges();

                _responseStream = await response.Content.ReadAsStreamAsync();
                var result = _fileSystem.SaveFile(DownloadPath, DownloadTask.FileName, TotalBytesToReceive);
                if (result.IsFailed)
                {
                    SetDownloadStatus(DownloadStatus.Error);
                    return result.ToResult();
                }
                _fileStream = result.Value;
                if (_fileStream == null)
                {
                    SetDownloadStatus(DownloadStatus.Error);
                    return Result.Fail($"The file stream was null with destination {DownloadPath}");
                }

                // Set Timings
                DownloadStartAt = DateTime.UtcNow;
                _progressTimer.Start();

                SetDownloadStatus(DownloadStatus.Downloading);

                _copyTask = _responseStream
                    .CopyToAsync(_fileStream, _progress, 81920, _cancellationToken.Token)
                    .ContinueWith(task =>
                    {
                        if (task.IsCanceled) return;
                        _responseStream.Dispose();
                        _fileStream.Dispose();
                    });

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

        protected virtual void OnDownloadProgressChanged(long bytesReceived)
        {
            var downloadProgress = new DownloadProgress
            {
                Id = DownloadTaskId,
                Status = DownloadTask.DownloadStatus.ToString(),
                Percentage = DataFormat.GetPercentage(bytesReceived, TotalBytesToReceive),
                DataReceived = bytesReceived,
                DataTotal = TotalBytesToReceive,
                DownloadSpeed = DownloadSpeed,
                TimeRemaining = (int) Math.Floor(BytesRemaining / (double) DownloadSpeed),
            };

            DownloadProgressChanged?.Invoke(this, downloadProgress);
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
                rangeList.Add(new DownloadRange(start, end));
            }

            return rangeList;
        }

        #endregion Methods

        #region Events

        public event EventHandler<DownloadProgress> DownloadProgressChanged;

        public event EventHandler<DownloadProgress> DownloadFileCompleted;

        public event EventHandler<DownloadStatus> DownloadStatusChanged;

        #endregion Events
    }
}