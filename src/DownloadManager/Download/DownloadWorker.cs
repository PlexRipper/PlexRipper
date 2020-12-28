using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using PlexRipper.Application.Common;
using PlexRipper.Domain;
using PlexRipper.DownloadManager.Common;
using Timer = System.Timers.Timer;

namespace PlexRipper.DownloadManager.Download
{
    /// <summary>
    /// The <see cref="DownloadWorker"/> is part of the multi-threaded <see cref="PlexDownloadClient"/>
    /// and will download a part of the <see cref="DownloadTask"/>.
    /// </summary>
    public class DownloadWorker
    {
        #region Fields

        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private readonly Subject<Result> _downloadWorkerError = new Subject<Result>();

        private readonly Subject<DownloadWorkerComplete> _downloadWorkerComplete = new Subject<DownloadWorkerComplete>();

        private readonly Subject<IDownloadWorkerProgress> _downloadWorkerProgress = new Subject<IDownloadWorkerProgress>();

        private readonly Subject<DownloadWorkerTask> _downloadWorkerTaskChanged = new Subject<DownloadWorkerTask>();

        private readonly IFileSystem _fileSystem;

        private readonly IHttpClientFactory _httpClientFactory;

        private int _count;

        private Task _downloadTask;

        private bool _isDownloading = true;

        private Timer _timer = new Timer(100);

        private Task _downloadProcess;

        private FileStream _fileStream;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DownloadWorker"/> class.
        /// </summary>
        /// <param name="downloadWorkerTask">The download task this worker will execute.</param>
        /// <param name="fileSystem">The filesystem used to store the downloaded data.</param>
        public DownloadWorker(DownloadWorkerTask downloadWorkerTask, IFileSystem fileSystem, IHttpClientFactory httpClientFactory)
        {
            DownloadWorkerTask = downloadWorkerTask;
            _fileSystem = fileSystem;
            _httpClientFactory = httpClientFactory;

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

        #region Observables

        public IObservable<Result> DownloadWorkerError => _downloadWorkerError.AsObservable();

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
            _downloadWorkerComplete.OnNext(complete);
        }

        private void SendDownloadWorkerError(Result errorResult)
        {
            if (errorResult.Errors.Any())
            {
                errorResult.Errors.First().Metadata.Add(nameof(DownloadWorker) + "Id", Id);
            }

            _downloadWorkerError.OnNext(errorResult);
        }

        private void StartDownloadClient() { }

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
            _downloadWorkerProgress.OnNext(
                new DownloadWorkerProgress(
                    Id,
                    BytesReceived,
                    DownloadWorkerTask.BytesRangeSize,
                    DownloadSpeed,
                    DownloadSpeedAverage));
        }

        #endregion

        #region Public

        #region Commands

        public Result Start()
        {
            Log.Debug($"Download worker {Id} start for {FileName}");

            // Create and check Filestream to which to download.
            var _fileStreamResult =
                _fileSystem.DownloadWorkerTempFileStream(DownloadWorkerTask.TempDirectory, FileName, DownloadWorkerTask.BytesRangeSize);
            if (_fileStreamResult.IsFailed)
            {
                SendDownloadWorkerError(_fileStreamResult);
                return _fileStreamResult.ToResult();
            }

            DownloadStartAt = DateTime.UtcNow;
            _downloadProcess = Task.Factory.StartNew(async () =>
            {
                try
                {
                    _fileStream = _fileStreamResult.Value;

                    // Is 0 when starting new and > 0 when resuming.
                    _fileStream.Position = DownloadWorkerTask.BytesReceived;

                    // Create download client
                    var client = _httpClientFactory.CreateClient("Default");
                    using var response = await client.SendAsync(new HttpRequestMessage
                    {
                        RequestUri = DownloadWorkerTask.Uri,
                        Method = HttpMethod.Get,
                        Headers =
                        {
                            Range = new RangeHeaderValue(DownloadWorkerTask.CurrentByte, DownloadWorkerTask.EndByte),
                        },
                    }, HttpCompletionOption.ResponseHeadersRead);


                    await using Stream responseStream = await response.Content.ReadAsStreamAsync();

                    byte[] buffer = new byte[4096];

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
                    SendDownloadWorkerError(Result.Fail(new ExceptionalError(e)).LogError());
                    throw;
                }
            }, TaskCreationOptions.LongRunning);
            return Result.Ok();
        }

        private async Task<Result> CancelDownloadProcess()
        {
            _isDownloading = false;

            //_cancellationTokenSource.Cancel();

            // Wait for it to gracefully end.
            await _downloadProcess;
            _timer.Stop();
            return Result.Ok();
        }

        /// <summary>
        /// Pauses the <see cref="DownloadWorker"/> by storing the current <see cref="DownloadWorkerTask"/> in the database.
        /// </summary>
        public async Task<Result> Pause()
        {
            var cancelResult = await CancelDownloadProcess();
            if (cancelResult.IsFailed)
            {
                return cancelResult.LogError();
            }

            Log.Debug($"Download worker {Id} paused for {FileName}");
            _downloadWorkerTaskChanged.OnNext(DownloadWorkerTask);
            return Result.Ok();
        }

        /// <summary>
        /// Stops the downloading.
        /// </summary>
        /// <returns>Is successful.</returns>
        public async Task<Result> Stop()
        {
            var cancelResult = await CancelDownloadProcess();
            if (cancelResult.IsFailed)
            {
                return cancelResult.LogError();
            }

            Log.Debug($"Download worker {Id} stopped for {FileName}");
            return Result.Ok(true);
        }

        #endregion

        /// <inheritdoc/>
        public async Task DisposeAsync()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _downloadProcess?.Dispose();
            if (_fileStream != null)
            {
                _fileStream.Close();
                await _fileStream.DisposeAsync();
                _fileStream = null;
            }

            // Dispose subscriptions
            _downloadWorkerComplete?.Dispose();
            _downloadWorkerProgress?.Dispose();
            _downloadWorkerError?.Dispose();
            _downloadWorkerTaskChanged?.Dispose();

            // Dispose timer
            _timer?.Dispose();
        }

        #endregion

        #endregion
    }
}