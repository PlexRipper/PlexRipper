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
using Serilog.Events;
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

        private readonly Subject<DownloadWorkerLog> _downloadWorkerLog = new();

        private readonly Subject<DownloadWorkerTask> _downloadWorkerUpdate = new();

        private readonly IFileSystem _fileSystem;

        private readonly IPlexRipperHttpClient _httpClient;

        private bool _isDownloading = true;

        private readonly Timer _timer = new Timer(100)
        {
            AutoReset = true,
        };

        public Task<Result> DownloadProcessTask { get; internal set; }

        private int _downloadSpeedLimit;

        private readonly CancellationTokenSource _cancellationToken = new CancellationTokenSource();

        private Stream _destinationStream;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DownloadWorker"/> class.
        /// </summary>
        /// <param name="downloadWorkerTask">The download task this worker will execute.</param>
        /// <param name="fileSystem">The filesystem used to store the downloaded data.</param>
        /// <param name="httpClient"></param>
        /// <param name="downloadSpeedLimit"></param>
        public DownloadWorker(
            DownloadWorkerTask downloadWorkerTask,
            IFileSystem fileSystem,
            IPlexRipperHttpClient httpClient,
            int downloadSpeedLimit = 0)
        {
            DownloadWorkerTask = downloadWorkerTask;
            _fileSystem = fileSystem;
            _httpClient = httpClient;

            _timer.Elapsed += (_, _) => { DownloadWorkerTask.ElapsedTime += (long)_timer.Interval; };
            _downloadSpeedLimit = downloadSpeedLimit;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the current <see cref="DownloadWorkerTask"/> being executed.
        /// </summary>
        public DownloadWorkerTask DownloadWorkerTask { get; }

        public string FileName => DownloadWorkerTask.TempFileName;

        /// <summary>
        /// The download worker id, which is the same as the <see cref="DownloadWorkerTask"/> Id.
        /// </summary>
        public int Id => DownloadWorkerTask.Id;

        #region Observables

        public IObservable<DownloadWorkerLog> DownloadWorkerLog => _downloadWorkerLog.AsObservable();

        public IObservable<DownloadWorkerTask> DownloadWorkerTaskUpdate =>
            _downloadWorkerUpdate
                .Sample(TimeSpan.FromMilliseconds(100))
                .AsObservable();

        #endregion

        #endregion

        #region Methods

        #region Private

        #region SendData

        private void SendDownloadWorkerLog(LogEventLevel logLevel, string message)
        {
            _downloadWorkerLog.OnNext(new DownloadWorkerLog
            {
                Message = message,
                LogLevel = logLevel,
                CreatedAt = DateTime.Now,
                DownloadWorkerTaskId = DownloadWorkerTask.Id,
            });
        }

        private void SetDownloadWorkerTaskChanged(DownloadStatus status)
        {
            if (DownloadWorkerTask.DownloadStatus == status)
            {
                return;
            }

            var log = $"Download worker {Id} with {FileName} changed status to {status}";
            Log.Debug(log);
            DownloadWorkerTask.DownloadStatus = status;
            SendDownloadWorkerLog(LogEventLevel.Information, log);
            SendDownloadWorkerUpdate();
        }

        #endregion

        private void SendDownloadWorkerError(Result errorResult)
        {
            if (errorResult.Errors.Any())
            {
                errorResult.Errors.First().Metadata.Add(nameof(DownloadWorker) + "Id", Id);
            }

            Log.Error($"Download worker {Id} with {FileName} had an error!");
            DownloadWorkerTask.DownloadStatus = DownloadStatus.Error;

            SendDownloadWorkerLog(LogEventLevel.Error, errorResult.ToString());
            SendDownloadWorkerUpdate();
        }

        private void SendDownloadWorkerUpdate()
        {
            _downloadWorkerUpdate.OnNext(DownloadWorkerTask);
        }

        #endregion

        #region Public

        #region Commands

        public Result Start()
        {
            Log.Debug($"Download worker {Id} start for {FileName}");

            // Create and check Filestream to which to download.
            var _fileStreamResult =
                _fileSystem.DownloadWorkerTempFileStream(DownloadWorkerTask.TempDirectory, FileName, DownloadWorkerTask.DataTotal);
            if (_fileStreamResult.IsFailed)
            {
                SendDownloadWorkerError(_fileStreamResult);
                return _fileStreamResult.ToResult();
            }

            try
            {
                SetDownloadWorkerTaskChanged(DownloadStatus.Downloading);
                DownloadProcessTask = Task.Run(() => DownloadProcessAsync(_fileStreamResult.Value), _cancellationToken.Token);
            }
            catch (TaskCanceledException e)
            {
                // Ignore cancellation exceptions
            }

            return Result.Ok();
        }

        private async Task<Result> DownloadProcessAsync(Stream destinationStream)
        {
            if (destinationStream is null)
            {
                return Result.Fail(new Error("Parameter \"destinationStream\" was null.")).LogError();
            }

            try
            {
                _destinationStream = destinationStream;

                // Is 0 when starting new and > 0 when resuming.
                destinationStream.Position = DownloadWorkerTask.BytesReceived;

                // Create download client
                using var response = await _httpClient.SendAsync(new HttpRequestMessage
                {
                    RequestUri = DownloadWorkerTask.Uri,
                    Method = HttpMethod.Get,
                    Headers =
                    {
                        Range = new RangeHeaderValue(DownloadWorkerTask.CurrentByte, DownloadWorkerTask.EndByte),
                    },
                }, HttpCompletionOption.ResponseHeadersRead);

                await using Stream responseStream = await response.Content.ReadAsStreamAsync();

                // Throttle the stream to enable download speed limiting
                var throttledStream = new ThrottledStream(responseStream, _downloadSpeedLimit);

                byte[] buffer = new byte[4096];

                _timer.Start();

                while (_isDownloading)
                {
                    throttledStream.SetThrottleSpeed(_downloadSpeedLimit);
                    int bytesRead = await throttledStream.ReadAsync(buffer, 0, buffer.Length, _cancellationToken.Token);
                    if (bytesRead > 0)
                    {
                        bytesRead = (int)Math.Min(DownloadWorkerTask.DataRemaining, bytesRead);
                    }

                    if (bytesRead <= 0)
                    {
                        SendDownloadWorkerUpdate();
                        SetDownloadWorkerTaskChanged(DownloadStatus.Completed);
                        break;
                    }

                    await destinationStream.WriteAsync(buffer, 0, bytesRead, _cancellationToken.Token);
                    await destinationStream.FlushAsync(_cancellationToken.Token);

                    DownloadWorkerTask.BytesReceived += bytesRead;
                    SendDownloadWorkerUpdate();
                }
            }
            catch (Exception e)
            {
                SendDownloadWorkerError(Result.Fail(new ExceptionalError(e)).LogError());
            }
            finally
            {
                _timer.Dispose();
                _downloadWorkerUpdate.OnCompleted();
                _downloadWorkerLog.OnCompleted();
            }

            return Result.Ok();
        }

        /// <summary>
        /// Stops the downloading.
        /// </summary>
        /// <returns>Is successful.</returns>
        public async Task<Result<DownloadWorkerTask>> StopAsync()
        {
            _isDownloading = false;

            // Wait for it to gracefully end.
            await DownloadProcessTask;
            _timer.Stop();
            SetDownloadWorkerTaskChanged(DownloadStatus.Stopped);

            return Result.Ok(DownloadWorkerTask);
        }

        /// <summary>
        /// Stops the downloading.
        /// </summary>
        /// <returns>Is successful.</returns>
        public async Task<Result<DownloadWorkerTask>> PauseAsync()
        {
            _isDownloading = false;

            // Wait for it to gracefully end.
            await DownloadProcessTask;
            _timer.Stop();
            SetDownloadWorkerTaskChanged(DownloadStatus.Paused);

            return Result.Ok(DownloadWorkerTask);
        }

        public void SetDownloadSpeedLimit(int speedInKb)
        {
            _downloadSpeedLimit = speedInKb;
        }

        #endregion

        /// <inheritdoc/>
        public void Dispose()
        {
            _destinationStream.Close();
        }

        #endregion

        #endregion
    }
}