using System.Net.Http.Headers;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Timer = System.Timers.Timer;

namespace PlexRipper.DownloadManager.DownloadClient;

/// <summary>
/// The <see cref="DownloadWorker"/> is part of the multi-threaded <see cref="PlexDownloadClient"/>
/// and will download a part of the <see cref="DownloadTask"/>.
/// </summary>
public class DownloadWorker
{
    #region Fields

    private readonly CancellationTokenSource _cancellationToken = new();

    private readonly Subject<DownloadWorkerLog> _downloadWorkerLog = new();

    private readonly Subject<DownloadWorkerTask> _downloadWorkerUpdate = new();

    private readonly IDownloadFileStream _downloadFileSystem;

    private readonly IPlexRipperHttpClient _httpClient;

    private readonly Timer _timer = new(100)
    {
        AutoReset = true,
    };

    private Stream _destinationStream;

    private int _downloadSpeedLimit;

    private bool _isDownloading = true;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="DownloadWorker"/> class.
    /// </summary>
    /// <param name="downloadWorkerTask">The download task this worker will execute.</param>
    /// <param name="downloadFileSystem">The filesystem used to store the downloaded data.</param>
    /// <param name="httpClient"></param>
    public DownloadWorker(
        DownloadWorkerTask downloadWorkerTask,
        IDownloadFileStream downloadFileSystem,
        IPlexRipperHttpClient httpClient)
    {
        DownloadWorkerTask = downloadWorkerTask;
        _downloadFileSystem = downloadFileSystem;
        _httpClient = httpClient;

        _timer.Elapsed += (_, _) => { DownloadWorkerTask.ElapsedTime += (long)_timer.Interval; };
    }

    #endregion

    #region Properties

    public Task<Result> DownloadProcessTask { get; internal set; }

    public IObservable<DownloadWorkerLog> DownloadWorkerLog => _downloadWorkerLog.AsObservable();

    /// <summary>
    /// Gets the current <see cref="DownloadWorkerTask"/> being executed.
    /// </summary>
    public DownloadWorkerTask DownloadWorkerTask { get; }

    public IObservable<DownloadWorkerTask> DownloadWorkerTaskUpdate => _downloadWorkerUpdate
        .Sample(TimeSpan.FromMilliseconds(100))
        .AsObservable();

    public string FileName => DownloadWorkerTask.FileName;

    /// <summary>
    /// The download worker id, which is the same as the <see cref="DownloadWorkerTask"/> Id.
    /// </summary>
    public int Id => DownloadWorkerTask.Id;

    #endregion

    #region Public Methods

    public Result Start()
    {
        Log.Debug($"Download worker {Id} start for {FileName}");

        // Create and check Filestream to which to download.
        var _fileStreamResult =
            _downloadFileSystem.CreateDownloadFileStream(DownloadWorkerTask.TempDirectory, FileName, DownloadWorkerTask.DataTotal);
        if (_fileStreamResult.IsFailed)
        {
            SendDownloadWorkerError(_fileStreamResult.ToResult());
            return _fileStreamResult.ToResult();
        }

        try
        {
            SetDownloadWorkerTaskChanged(DownloadStatus.Downloading);
            DownloadProcessTask = Task.Run(() => DownloadProcessAsync(_fileStreamResult.Value), _cancellationToken.Token);
        }
        catch (TaskCanceledException)
        {
            // Ignore cancellation exceptions
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
        if (DownloadProcessTask is not null)
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
        if (DownloadProcessTask is not null)
            await DownloadProcessTask;

        _timer.Stop();
        SetDownloadWorkerTaskChanged(DownloadStatus.Paused);

        return Result.Ok(DownloadWorkerTask);
    }

    public void SetDownloadSpeedLimit(int speedInKb)
    {
        _downloadSpeedLimit = speedInKb;
    }

    public void Dispose()
    {
        _destinationStream?.Close();
    }

    #endregion

    #region Private Methods

    private async Task<Result> DownloadProcessAsync(Stream destinationStream)
    {
        if (destinationStream is null)
            return ResultExtensions.IsNull(nameof(destinationStream)).LogWarning();

        try
        {
            _destinationStream = destinationStream;

            // Is 0 when starting new and > 0 when resuming.
            _destinationStream.Position = DownloadWorkerTask.BytesReceived;

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

            await using var responseStream = await response.Content.ReadAsStreamAsync();

            // Throttle the stream to enable download speed limiting
            var throttledStream = new ThrottledStream(responseStream, _downloadSpeedLimit);

            // Buffer is based on: https://stackoverflow.com/a/39355385/8205497
            var buffer = new byte[524288000];

            _timer.Start();

            while (_isDownloading)
            {
                throttledStream.SetThrottleSpeed(_downloadSpeedLimit);
                var bytesRead = await throttledStream.ReadAsync(buffer, 0, buffer.Length, _cancellationToken.Token);
                if (bytesRead > 0)
                    bytesRead = (int)Math.Min(DownloadWorkerTask.DataRemaining, bytesRead);

                if (bytesRead <= 0)
                {
                    SendDownloadWorkerUpdate();
                    SetDownloadWorkerTaskChanged(DownloadStatus.DownloadFinished);
                    break;
                }

                await _destinationStream.WriteAsync(buffer.AsMemory(0, bytesRead), _cancellationToken.Token);
                await _destinationStream.FlushAsync(_cancellationToken.Token);

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
            await _destinationStream.DisposeAsync();
            _downloadWorkerUpdate.OnCompleted();
            _downloadWorkerLog.OnCompleted();
        }

        return Result.Ok();
    }

    private void SendDownloadWorkerError(Result errorResult)
    {
        if (errorResult.Errors.Any() && !errorResult.Errors[0].Metadata.ContainsKey(nameof(DownloadWorker) + "Id"))
            errorResult.Errors[0].Metadata.Add(nameof(DownloadWorker) + "Id", Id);

        Log.Error($"Download worker {Id} with {FileName} had an error!");
        DownloadWorkerTask.DownloadStatus = DownloadStatus.Error;

        SendDownloadWorkerLog(NotificationLevel.Error, errorResult.ToString());
        SendDownloadWorkerUpdate();
    }

    private void SendDownloadWorkerLog(NotificationLevel logLevel, string message)
    {
        _downloadWorkerLog.OnNext(new DownloadWorkerLog
        {
            Message = message,
            LogLevel = logLevel,
            CreatedAt = DateTime.Now,
            DownloadWorkerTaskId = DownloadWorkerTask.Id,
        });
    }

    private void SendDownloadWorkerUpdate()
    {
        _downloadWorkerUpdate.OnNext(DownloadWorkerTask);
    }

    private void SetDownloadWorkerTaskChanged(DownloadStatus status)
    {
        if (DownloadWorkerTask.DownloadStatus == status)
            return;

        var log = $"Download worker {Id} with {FileName} changed status to {status}";
        Log.Debug(log);
        DownloadWorkerTask.DownloadStatus = status;
        SendDownloadWorkerLog(NotificationLevel.Information, log);
        SendDownloadWorkerUpdate();
    }

    #endregion
}