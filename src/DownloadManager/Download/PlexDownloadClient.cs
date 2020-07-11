using FluentResults;
using PlexRipper.Application.Common.Interfaces.DownloadManager;
using PlexRipper.Application.Common.Interfaces.FileSystem;
using PlexRipper.Application.Common.Interfaces.Settings;
using PlexRipper.Domain;
using PlexRipper.Domain.Entities;
using PlexRipper.Domain.Types;
using PlexRipper.DownloadManager.Common;
using PlexRipper.PlexApi.Api;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PlexRipper.DownloadManager.Download
{
    public class PlexDownloadClient : HttpClient
    {

        #region Fields

        private readonly CancellationTokenSource _cancellationToken = new CancellationTokenSource();
        private readonly IFileSystem _fileSystem;
        private readonly Progress<long> _progress = new Progress<long>();
        private Task _copyTask;
        private IDownloadManager _downloadManager;
        private Stream _fileStream;
        private Stream _responseStream;
        private IUserSettings _userSettings;
        private List<long> _bytesReceivedHistory = new List<long>();
        private DateTime _lastNotificationTime = DateTime.UtcNow;
        private bool _calculateDownloadSpeedLock = false;
        #endregion Fields

        #region Constructors

        public PlexDownloadClient(DownloadTask downloadTask, IDownloadManager downloadManager, IUserSettings userSettings, IFileSystem fileSystem)
        {

            _downloadManager = downloadManager;
            _userSettings = userSettings;
            _fileSystem = fileSystem;
            DownloadTask = downloadTask;
            AddHeaders();

            // Setup progress changed event
            _progress.ProgressChanged += (sender, l) =>
            {
                BytesReceived = l;
                CalculateDownloadSpeed(l);
                OnDownloadProgressChanged(l);
            };
        }

        #endregion Constructors

        #region Properties

        public long BytesReceived { get; internal set; }

        /// <summary>
        /// The ClientId is always the same id that is assigned to the <see cref="DownloadTask"/>
        /// </summary>
        public int ClientId => DownloadTask.Id;

        // Size of downloaded data which was written to the local file
        public long DownloadedSize { get; set; }

        public string DownloadPath => _fileSystem.ToAbsolutePath(DownloadTask.FolderPath?.Directory);

        public int DownloadSpeed { get; internal set; }

        public DateTime DownloadStartAt { get; internal set; }

        public DownloadTask DownloadTask { get; internal set; }

        public TimeSpan ElapsedTime => DateTime.UtcNow.Subtract(DownloadStartAt);
        public TimeSpan ElapsedTimeDownloadSpeed => DateTime.UtcNow.Subtract(_lastNotificationTime);



        public decimal Percentage { get; internal set; }

        public long TotalBytesToReceive { get; internal set; }

        #endregion Properties

        #region Methods

        private void AddHeaders()
        {
            foreach ((string key, string value) in PlexHeaderData.GetBasicHeaders)
            {
                this.DefaultRequestHeaders.Add(key, value);
            }
        }

        private void CalculateDownloadSpeed(long bytesReceived)
        {
            // Only run this once a second
            if (ElapsedTime.TotalMilliseconds < 1000 && !_calculateDownloadSpeedLock)
            {
                return;
            }
            _calculateDownloadSpeedLock = true;

            // Ensure there are only 100 entries
            _bytesReceivedHistory.Add(bytesReceived);
            if (_bytesReceivedHistory.Count > 100)
            {
                _bytesReceivedHistory.RemoveAt(0);
            }
            var sizeDiffList = new List<long>();
            for (int i = 0; i < _bytesReceivedHistory.Count; i += 2)
            {
                if (_bytesReceivedHistory.Count > i + 1)
                {
                    sizeDiffList.Add(Math.Abs(_bytesReceivedHistory[i] - _bytesReceivedHistory[i + 1]));
                }
            }

            if (sizeDiffList.Count > 0)
            {
                var sizeDiff = sizeDiffList.Last();

                DownloadSpeed = (int)Math.Floor(sizeDiff / ElapsedTime.TotalSeconds);
            }

            // TODO Add average speed
            _lastNotificationTime = DateTime.UtcNow;
            _calculateDownloadSpeedLock = false;
        }

        /// <summary>
        /// Start the download of the DownloadTask passed during the construction.
        /// </summary>
        /// <returns></returns>
        public async Task<Result> StartAsync()
        {
            Log.Debug(DownloadTask.DownloadUrl);
            try
            {
                // Source: https://stackoverflow.com/a/48190014/8205497
                var response = await GetAsync(DownloadTask.DownloadUri, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                TotalBytesToReceive = response.Content.Headers.ContentLength ?? -1L;
                if (TotalBytesToReceive <= 0)
                {
                    return Result.Fail("File size could not be determined of the media that will be downloaded");
                }

                _responseStream = await response.Content.ReadAsStreamAsync();
                var result = _fileSystem.SaveFile(DownloadPath, DownloadTask.FileName, TotalBytesToReceive);
                if (result.IsFailed)
                {
                    return result;
                }
                _fileStream = result.Value;

                // Set Timings
                DownloadStartAt = DateTime.UtcNow;
                _lastNotificationTime = DateTime.UtcNow;

                _copyTask = _responseStream
                    .CopyToAsync(_fileStream, _progress, 81920, _cancellationToken.Token)
                    .ContinueWith(task =>
                    {
                        if (task.IsCanceled) return;
                        _responseStream.Dispose();
                        _fileStream.Dispose();
                    });

                return Result.Ok();

            }
            catch (Exception e)
            {
                var msg = $"Could not download {DownloadTask.FileName} from {DownloadTask.DownloadUrl}";
                Log.Error(e, msg);
                var result = Result.Fail(new ExceptionalError(e));
                result.Errors.Add(new Error(msg));
                return result;
            }
            finally
            {
                // TODO Delete file downloaded
                Cancel();
            }
        }

        public bool Cancel()
        {
            // TODO using exception like this might be dangerous
            try
            {
                _cancellationToken.Cancel();
                _responseStream.Dispose();
                _fileStream.Dispose();
            }
            catch (Exception e)
            {
                Log.Error(e, $"Failed to cancel downloadTask with client id: {ClientId}");
                return false;
            }
            return true;
        }
        protected virtual void OnDownloadProgressChanged(long bytesReceived)
        {
            var newPercentage = DataFormat.GetPercentage(bytesReceived, TotalBytesToReceive);
            // Only fire event when percentage has changed
            if (newPercentage != Percentage)
            {
                Percentage = newPercentage;

                var downloadProgress = new DownloadProgress
                {
                    Id = ClientId,
                    Status = DownloadTask.DownloadStatus,
                    Percentage = newPercentage,
                    DataReceived = bytesReceived,
                    DataTotal = TotalBytesToReceive,
                    DownloadSpeed = DownloadSpeed.ToString()
                };

                DownloadProgressChanged?.Invoke(this, downloadProgress);
            }
        }

        #endregion Methods
        #region Events

        public event EventHandler<DownloadProgress> DownloadProgressChanged;

        #endregion Events
    }
}
