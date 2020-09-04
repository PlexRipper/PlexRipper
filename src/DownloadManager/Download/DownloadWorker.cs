using FluentResults;
using PlexRipper.Domain;
using PlexRipper.Domain.Common;
using PlexRipper.Domain.Entities;
using PlexRipper.Domain.Enums;
using PlexRipper.DownloadManager.Common;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using PlexRipper.Application.Common;

namespace PlexRipper.DownloadManager.Download
{
    /// <summary>
    /// The <see cref="DownloadWorker"/> is part of the multi-threaded <see cref="PlexDownloadClient"/>
    /// and will download a part of the <see cref="DownloadTask"/>.
    /// </summary>
    public class DownloadWorker : IDisposable
    {
        #region Fields

        private readonly CancellationToken _cancellationToken;
        private readonly Subject<DownloadStatusChanged> _downloadStatusChanged = new Subject<DownloadStatusChanged>();

        private readonly Subject<DownloadWorkerComplete> _downloadWorkerComplete = new Subject<DownloadWorkerComplete>();
        private readonly Subject<IDownloadWorkerProgress> _downloadWorkerProgress = new Subject<IDownloadWorkerProgress>();
        private readonly Subject<DownloadWorkerTask> _downloadWorkerTaskChanged = new Subject<DownloadWorkerTask>();

        private readonly IFileSystem _fileSystem;
        private int _count = 0;
        private Task _downloadTask;

        private FileStream _fileStream;
        private bool _isDownloading = true;
        private TimeSpan _lastProgress = TimeSpan.Zero;
        private System.Timers.Timer _timer = new System.Timers.Timer(100);

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DownloadWorker"/> class.
        /// </summary>
        /// <param name="downloadWorkerTask">The download task this worker will execute.</param>
        /// <param name="fileSystem">The filesystem used to store the downloaded data.</param>
        /// <param name="cancellationToken">Token to cancel.</param>
        public DownloadWorker(DownloadWorkerTask downloadWorkerTask, IFileSystem fileSystem, CancellationToken cancellationToken)
        {
            DownloadWorkerTask = downloadWorkerTask;

            _fileSystem = fileSystem;
            _cancellationToken = cancellationToken;

            _timer.AutoReset = true;
            _timer.Elapsed += (sender, args) => { DownloadWorkerTask.ElapsedTime += (int)_timer.Interval; };
        }

        #endregion

        #region Properties

        public long BytesReceived
        {
            get => DownloadWorkerTask.BytesReceived;
            set => DownloadWorkerTask.BytesReceived = value;
        }

        public int DownloadSpeed => DataFormat.GetDownloadSpeed(BytesReceived, ElapsedTime.TotalSeconds);

        public int DownloadSpeedAverage { get; set; }

        /// <summary>
        /// Bytes per second download speed
        /// </summary>
        public DateTime DownloadStartAt { get; internal set; }

        public DownloadWorkerTask DownloadWorkerTask { get; }


        public TimeSpan ElapsedTime => DateTime.UtcNow.Subtract(DownloadStartAt);

        public string FileName => DownloadWorkerTask.TempFileName;

        public string FilePath => DownloadWorkerTask.TempFilePath;

        /// <summary>
        /// The download worker id, which is the same as the <see cref="DownloadWorkerTask"/> Id.
        /// </summary>
        public int Id => DownloadWorkerTask.Id;

        public DownloadStatus Status { get; internal set; }

        public int TimeElapsed
        {
            get => DownloadWorkerTask.ElapsedTime;
            set => DownloadWorkerTask.ElapsedTime = value;
        }

        #region Observables

        public IObservable<DownloadStatusChanged> DownloadStatusChanged => _downloadStatusChanged.AsObservable();

        public IObservable<DownloadWorkerComplete> DownloadWorkerComplete => _downloadWorkerComplete.AsObservable();

        public IObservable<IDownloadWorkerProgress> DownloadWorkerProgress => _downloadWorkerProgress.AsObservable();

        public IObservable<DownloadWorkerTask> DownloadWorkerTaskChanged => _downloadWorkerTaskChanged.AsObservable();

        #endregion

        #endregion

        #region Methods

        #region Private

        private void Complete()
        {
            var complete = new DownloadWorkerComplete(Id)
            {
                FilePath = FilePath,
                FileName = FileName,
                DownloadSpeedAverage = DownloadSpeedAverage,
                BytesReceived = BytesReceived,
                BytesReceivedGoal = DownloadWorkerTask.BytesRangeSize,
            };
            SetDownloadStatus(DownloadStatus.Completed);
            _downloadWorkerComplete.OnNext(complete);
        }

        private void SetDownloadStatus(DownloadStatus downloadStatus)
        {
            Status = downloadStatus;
            _downloadStatusChanged.OnNext(new DownloadStatusChanged(Id, downloadStatus));
        }


        private Task StartDownloadClient()
        {
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    using var httpClient = new HttpClient();
                    using var requestMessage = new HttpRequestMessage(HttpMethod.Get, DownloadWorkerTask.Uri);
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(DownloadWorkerTask.Uri);
                    request.AddRange(DownloadWorkerTask.StartByte, DownloadWorkerTask.EndByte);
                    var response = (HttpWebResponse)request.GetResponse();

                    using var responseStream = response.GetResponseStream();
                    var buffer = new byte[1024 * 2];

                    SetDownloadStatus(DownloadStatus.Downloading);
                    _timer.Start();
                    while (_isDownloading)
                    {
                        if (_cancellationToken.IsCancellationRequested)
                        {
                            SetDownloadStatus(DownloadStatus.Stopping);
                            Log.Information($"Canceling Download worker {Id}");
                            Stop();
                            break;
                        }

                        int bytesRead = responseStream.Read(buffer, 0, buffer.Length);
                        if (bytesRead > 0)
                        {
                            bytesRead = (int)Math.Min(DownloadWorkerTask.BytesRangeSize - BytesReceived, bytesRead);
                        }

                        if (bytesRead <= 0)
                        {
                            UpdateProgress();
                            Complete();
                            break;
                        }

                        BytesReceived += bytesRead;

                        // TODO Add snapshot every n mb downloaded e.g.: _downloadWorkerTaskChanged.OnNext(DownloadWorkerTask);
                        _fileStream.Write(buffer, 0, bytesRead);
                        _fileStream.Flush();

                        UpdateProgress();
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e);
                    SetDownloadStatus(DownloadStatus.Error);
                    throw;
                }
            }, TaskCreationOptions.LongRunning);
        }

        private void UpdateAverage(int newValue)
        {
            if (_count == 0)
            {
                DownloadSpeedAverage = newValue;
                _count++;
                return;
            }

            DownloadSpeedAverage = (DownloadSpeedAverage * (_count - 1) / _count) + (newValue / _count);
        }

        private void UpdateProgress()
        {
            UpdateAverage(DownloadSpeed);
            _downloadWorkerProgress.OnNext(
                new DownloadWorkerProgress(
                    Id,
                    BytesReceived,
                    DownloadWorkerTask.BytesRangeSize,
                    DownloadSpeed,
                    DownloadSpeedAverage));
            _lastProgress = ElapsedTime;
        }

        #endregion

        #region Public

        #region Commands

        public Result<Task> Start()
        {
            Log.Debug($"Download worker {Id} start for {FileName}");
            SetDownloadStatus(DownloadStatus.Initialized);
            var fileStreamResult =
                _fileSystem.SaveFile(DownloadWorkerTask.TempDirectory, FileName, DownloadWorkerTask.BytesRangeSize);
            if (fileStreamResult.IsFailed)
            {
                SetDownloadStatus(DownloadStatus.Error);
                return fileStreamResult.ToResult();
            }

            _fileStream = fileStreamResult.Value;
            _fileStream.Position = 0;

            DownloadStartAt = DateTime.UtcNow;
            _downloadTask = Task.Factory.StartNew(StartDownloadClient, TaskCreationOptions.LongRunning);
            return Result.Ok(_downloadTask);
        }

        /// <summary>
        /// Stops the downloading, removes all temp files and disposes itself.
        /// </summary>
        /// <returns>Is successful.</returns>
        public Result<bool> Stop()
        {
            try
            {
                SetDownloadStatus(DownloadStatus.Stopped);
                File.Delete(FilePath);
                Dispose();
                return Result.Ok(true);
            }
            catch (Exception e)
            {
                return Result.Fail(new ExceptionalError(e)).LogError();
            }
        }

        /// <summary>
        /// Pauses the <see cref="DownloadWorker"/>.
        /// </summary>
        public void Pause()
        {
            _isDownloading = false;
            _timer.Stop();
            SetDownloadStatus(DownloadStatus.Paused);
            _downloadWorkerTaskChanged.OnNext(DownloadWorkerTask);
        }

        /// <summary>
        /// Resumes the <see cref="DownloadWorker"/>.
        /// </summary>
        public void Resume()
        {
            _isDownloading = true;
            StartDownloadClient();
        }

        #endregion

        /// <inheritdoc/>
        public void Dispose()
        {
            _downloadWorkerComplete?.Dispose();
            _downloadWorkerProgress?.Dispose();
            _downloadStatusChanged?.Dispose();
            _timer?.Dispose();
            _fileStream?.Close();
            _fileStream?.Dispose();
        }

        #endregion

        #endregion
    }
}