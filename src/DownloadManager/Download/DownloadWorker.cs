using FluentResults;
using PlexRipper.Application.Common.Interfaces.DownloadManager;
using PlexRipper.Application.Common.Interfaces.FileSystem;
using PlexRipper.Domain;
using PlexRipper.Domain.Common;
using PlexRipper.Domain.Entities;
using PlexRipper.Domain.Enums;
using PlexRipper.DownloadManager.Common;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace PlexRipper.DownloadManager.Download
{
    public class DownloadWorker : IDisposable
    {
        #region Fields

        private readonly Subject<DownloadWorkerComplete> _downloadWorkerComplete = new Subject<DownloadWorkerComplete>();
        private readonly Subject<IDownloadWorkerProgress> _downloadWorkerProgress = new Subject<IDownloadWorkerProgress>();
        private readonly Subject<DownloadStatusChanged> _statusChanged = new Subject<DownloadStatusChanged>();

        private readonly IFileSystem _fileSystem;
        private readonly CancellationToken _cancellationToken;
        private int _count = 0;

        private FileStream _fileStream;
        private Task _downloadTask;
        private TimeSpan _lastProgress = TimeSpan.Zero;

        #endregion Fields

        #region Constructors

        public DownloadWorker(DownloadRange downloadRange, IFileSystem fileSystem, CancellationToken cancellationToken)
        {
            if (downloadRange != null)
            {
                DownloadRange = downloadRange;
            }
            _fileSystem = fileSystem;
            _cancellationToken = cancellationToken;
        }

        #endregion Constructors

        #region Properties

        public long BytesReceived { get; internal set; }
        public DownloadRange DownloadRange { get; }
        public int DownloadSpeed => DataFormat.GetDownloadSpeed(BytesReceived, ElapsedTime.TotalSeconds);
        public int DownloadSpeedAverage { get; set; }

        /// <summary>
        /// Bytes per second download speed
        /// </summary>
        public DateTime DownloadStartAt { get; internal set; }



        public TimeSpan ElapsedTime => DateTime.UtcNow.Subtract(DownloadStartAt);
        public string FileName => DownloadRange.TempFileName;
        public string FilePath => DownloadRange.TempFilePath;

        /// <summary>
        /// The download worker id, which is the same as the <see cref="DownloadRange"/> Id.
        /// </summary>
        public int Id => DownloadRange.Id;

        public DownloadStatus Status { get; internal set; }

        #region Observables

        public IObservable<DownloadStatusChanged> DownloadStatusChanged => _statusChanged.AsObservable();
        public IObservable<DownloadWorkerComplete> DownloadWorkerComplete => _downloadWorkerComplete.AsObservable();
        public IObservable<IDownloadWorkerProgress> DownloadWorkerProgress => _downloadWorkerProgress.AsObservable();

        #endregion
        #endregion Properties

        #region Methods

        #region Commands

        public Result<Task> Start()
        {
            Log.Debug($"Download worker {Id} start for {FileName}");
            SetDownloadStatus(DownloadStatus.Initialized);
            var fileStreamResult =
                _fileSystem.SaveFile(DownloadRange.TempDirectory, FileName, DownloadRange.RangeSize);
            if (fileStreamResult.IsFailed)
            {
                SetDownloadStatus(DownloadStatus.Error);
                return fileStreamResult.ToResult();
            }
            _fileStream = fileStreamResult.Value;
            SetDownloadStatus(DownloadStatus.Starting);
            _downloadTask = Task.Factory.StartNew(DownloadClient, TaskCreationOptions.LongRunning);
            return Result.Ok(_downloadTask);
        }

        public Result<bool> Stop()
        {
            try
            {
                File.Delete(FilePath);
                SetDownloadStatus(DownloadStatus.Stopped);
                Dispose();
                return Result.Ok(true);
            }
            catch (Exception e)
            {
                Log.Error(e);
                throw;
            }
        }

        #endregion

        private void Complete()
        {
            var complete = new DownloadWorkerComplete(Id)
            {
                FilePath = FilePath,
                FileName = FileName,
                DownloadSpeedAverage = DownloadSpeedAverage
            };
            SetDownloadStatus(DownloadStatus.Completed);
            _downloadWorkerComplete.OnNext(complete);
            Dispose();
        }

        private void DownloadClient()
        {
            try
            {
                using var httpClient = new HttpClient();
                using var requestMessage = new HttpRequestMessage(HttpMethod.Get, DownloadRange.Uri);
                HttpWebRequest request = (HttpWebRequest) WebRequest.Create(DownloadRange.Uri);
                request.AddRange(DownloadRange.StartByte, DownloadRange.EndByte);
                var response = (HttpWebResponse) request.GetResponse();

                // var response = await httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, _cancellationToken);
                using var _responseStream = response.GetResponseStream();
                var buffer = new byte[2048];
                DownloadStartAt = DateTime.UtcNow;
                SetDownloadStatus(DownloadStatus.Downloading);
                while (true)
                {
                    if (_cancellationToken.IsCancellationRequested)
                    {
                        SetDownloadStatus(DownloadStatus.Stopping);
                        Log.Information($"Canceling Download worker {Id}");
                        Stop();
                        break;
                    }
                    int bytesRead = _responseStream.Read(buffer, 0, buffer.Length);
                    if (response.ContentLength > 0)
                    {
                        bytesRead = (int) Math.Min(DownloadRange.RangeSize - DownloadRange.BytesReceived, bytesRead);
                    }
                    if (bytesRead <= 0)
                    {
                        UpdateProgress();
                        Complete();
                        break;
                    }
                    BytesReceived += bytesRead;
                    _fileStream.Write(buffer, 0, bytesRead);
                    _fileStream.Flush();
                    if (ElapsedTime.Subtract(_lastProgress).TotalMilliseconds >= 500d)
                    {
                        UpdateProgress();
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
                SetDownloadStatus(DownloadStatus.Error);
                throw;
            }
        }

        private void SetDownloadStatus(DownloadStatus downloadStatus)
        {
            Status = downloadStatus;
            _statusChanged.OnNext(new DownloadStatusChanged(Id, downloadStatus));
        }

        private void UpdateAverage(int newValue)
        {
            if (_count == 0)
            {
                DownloadSpeedAverage = newValue;
                _count++;
                return;
            }
            DownloadSpeedAverage = DownloadSpeedAverage * (_count - 1) / _count + newValue / _count;
        }

        private void UpdateProgress()
        {
            UpdateAverage(DownloadSpeed);
            _downloadWorkerProgress.OnNext(new DownloadWorkerProgress(Id, BytesReceived, DownloadRange.RangeSize,
                DownloadSpeed,
                DownloadSpeedAverage));
            _lastProgress = ElapsedTime;
        }

        #endregion Methods


        public void Dispose()
        {
            _downloadWorkerComplete?.Dispose();
            _downloadWorkerProgress?.Dispose();
            _statusChanged?.Dispose();
            _fileStream?.Dispose();
           // _downloadTask?.Dispose();
        }
    }
}