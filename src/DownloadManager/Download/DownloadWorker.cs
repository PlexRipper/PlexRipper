using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using PlexRipper.Application.Common;
using PlexRipper.Domain;
using PlexRipper.DownloadManager.Common;

namespace PlexRipper.DownloadManager.Download
{
    /// <summary>
    /// The <see cref="DownloadWorker"/> is part of the multi-threaded <see cref="PlexDownloadClient"/>
    /// and will download a part of the <see cref="DownloadTask"/>.
    /// </summary>
    public class DownloadWorker : IDisposable
    {
        #region Fields

        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private readonly Subject<DownloadStatusChanged> _downloadStatusChanged = new Subject<DownloadStatusChanged>();
        private readonly Subject<DownloadWorkerComplete> _downloadWorkerComplete = new Subject<DownloadWorkerComplete>();
        private readonly Subject<IDownloadWorkerProgress> _downloadWorkerProgress = new Subject<IDownloadWorkerProgress>();
        private readonly Subject<DownloadWorkerTask> _downloadWorkerTaskChanged = new Subject<DownloadWorkerTask>();

        private readonly IFileSystem _fileSystem;
        private int _count = 0;
        private Task _downloadTask;

        private FileStream _fileStream;
        private bool _isDownloading = true;
        private System.Timers.Timer _timer = new System.Timers.Timer(100);

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DownloadWorker"/> class.
        /// </summary>
        /// <param name="downloadWorkerTask">The download task this worker will execute.</param>
        /// <param name="fileSystem">The filesystem used to store the downloaded data.</param>
        public DownloadWorker(DownloadWorkerTask downloadWorkerTask, IFileSystem fileSystem)
        {
            DownloadWorkerTask = downloadWorkerTask;
            _fileSystem = fileSystem;

            _timer.AutoReset = true;
            _timer.Elapsed += (sender, args) => { DownloadWorkerTask.ElapsedTime += (long)_timer.Interval; };
        }

        #endregion

        #region Properties

        public long BytesReceived
        {
            get => DownloadWorkerTask.BytesReceived;
            set => DownloadWorkerTask.BytesReceived = value;
        }

        public int DownloadSpeed => DataFormat.GetDownloadSpeed(BytesReceived, DownloadWorkerTask.ElapsedTimeSpan.TotalSeconds);

        public int DownloadSpeedAverage { get; set; }

        /// <summary>
        /// Gets the <see cref="DateTime"/> from when this <see cref="DownloadWorkerTask"/> was started.
        /// </summary>
        public DateTime DownloadStartAt { get; internal set; }

        /// <summary>
        /// Gets the current <see cref="DownloadWorkerTask"/> being executed.
        /// </summary>
        public DownloadWorkerTask DownloadWorkerTask { get; }


        public string FileName => DownloadWorkerTask.TempFileName;

        public string FilePath => DownloadWorkerTask.TempFilePath;

        /// <summary>
        /// The download worker id, which is the same as the <see cref="DownloadWorkerTask"/> Id.
        /// </summary>
        public int Id => DownloadWorkerTask.Id;

        public DownloadStatus Status { get; internal set; }

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
            return Task.Factory.StartNew(async () =>
            {
                try
                {
                    using var httpClient = new HttpClient();
                    using var requestMessage = new HttpRequestMessage(HttpMethod.Get, DownloadWorkerTask.Uri);
                    var request = (HttpWebRequest)WebRequest.Create(DownloadWorkerTask.Uri);
                    request.AddRange(DownloadWorkerTask.CurrentByte, DownloadWorkerTask.EndByte);
                    var response = (HttpWebResponse)request.GetResponse();

                    await using Stream responseStream = response.GetResponseStream();
                    if (responseStream == null)
                    {
                        Log.Error($"The ResponseStream was null");
                        return;
                    }

                    byte[] buffer = new byte[4096];

                    SetDownloadStatus(DownloadStatus.Downloading);
                    _timer.Start();
                    while (_isDownloading)
                    {
                        int bytesRead = await responseStream.ReadAsync(buffer, 0, buffer.Length, _cancellationTokenSource.Token);
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

                        // TODO Add snapshot every n mb downloaded e.g.: _downloadWorkerTaskChanged.OnNext(DownloadWorkerTask);
                        await _fileStream.WriteAsync(buffer, 0, bytesRead, _cancellationTokenSource.Token);
                        await _fileStream.FlushAsync(_cancellationTokenSource.Token);

                        BytesReceived += bytesRead;
                        UpdateProgress();
                    }
                }
                catch (TaskCanceledException e)
                {
                    // Ignore cancellation exceptions
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
        }

        private void CloseFileStream()
        {
            _fileStream?.Close();
        }

        #endregion

        #region Public

        #region Commands

        public Result<Task> Start()
        {
            Log.Debug($"Download worker {Id} start for {FileName}");
            SetDownloadStatus(DownloadStatus.Initialized);
            var fileStreamResult =
                _fileSystem.DownloadWorkerTempFileStream(DownloadWorkerTask.TempDirectory, FileName, DownloadWorkerTask.BytesRangeSize);
            if (fileStreamResult.IsFailed)
            {
                SetDownloadStatus(DownloadStatus.Error);
                return fileStreamResult.ToResult();
            }

            _fileStream = fileStreamResult.Value;

            // Is 0 when starting new and > 0 when resuming.
            _fileStream.Position = DownloadWorkerTask.BytesReceived;

            DownloadStartAt = DateTime.UtcNow;
            _downloadTask = StartDownloadClient();
            return Result.Ok(_downloadTask);
        }

        /// <summary>
        /// Stops the downloading and removes all temp files.
        /// </summary>
        /// <returns>Is successful.</returns>
        public Result<bool> Stop()
        {
            try
            {
                SetDownloadStatus(DownloadStatus.Stopped);
                File.Delete(FilePath);
                return Result.Ok(true);
            }
            catch (Exception e)
            {
                return Result.Fail(new ExceptionalError(e)).LogError();
            }
        }

        /// <summary>
        /// Pauses the <see cref="DownloadWorker"/> by storing the current <see cref="DownloadWorkerTask"/> in the database.
        /// </summary>
        public void Pause()
        {
            _timer.Stop();
            _isDownloading = false;
            _cancellationTokenSource.Cancel();
            _downloadWorkerTaskChanged.OnNext(DownloadWorkerTask);
        }

        #endregion

        /// <inheritdoc/>
        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();

            // Dispose subscriptions
            _downloadWorkerComplete?.Dispose();
            _downloadWorkerProgress?.Dispose();
            _downloadStatusChanged?.Dispose();
            _downloadWorkerTaskChanged?.Dispose();

            // Dispose timer
            _timer?.Dispose();

            // Dispose FileStream
            CloseFileStream();
        }

        #endregion

        #endregion
    }
}