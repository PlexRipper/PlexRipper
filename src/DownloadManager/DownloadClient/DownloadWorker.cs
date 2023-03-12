using System.Net.Http.Headers;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using ByteSizeLib;
using Data.Contracts;
using Logging.Interface;
using RestSharp;
using Timer = System.Timers.Timer;

namespace PlexRipper.DownloadManager;

/// <summary>
/// The <see cref="DownloadWorker"/> is part of the multi-threaded <see cref="PlexDownloadClient"/>
/// and will download a part of the <see cref="DownloadTask"/>.
/// </summary>
public class DownloadWorker : IDisposable
{
    #region Fields

    private readonly Subject<DownloadWorkerLog> _downloadWorkerLog = new();

    private readonly Subject<DownloadWorkerTask> _downloadWorkerUpdate = new();

    private readonly ILog<DownloadWorker> _log;
    private readonly IMediator _mediator;

    private readonly IDownloadFileStream _downloadFileSystem;

    private readonly RestClient _httpClient;

    private readonly Timer _timer = new(100)
    {
        AutoReset = true,
    };

    private int _downloadSpeedLimit;

    private bool _isDownloading = true;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="DownloadWorker"/> class.
    /// </summary>
    /// <param name="log"></param>
    /// <param name="mediator"></param>
    /// <param name="downloadWorkerTask">The download task this worker will execute.</param>
    /// <param name="downloadFileSystem">The filesystem used to store the downloaded data.</param>
    /// <param name="httpClientFactory"></param>
    public DownloadWorker(
        ILog<DownloadWorker> log,
        IMediator mediator,
        DownloadWorkerTask downloadWorkerTask,
        IDownloadFileStream downloadFileSystem,
        IHttpClientFactory httpClientFactory
    )
    {
        _log = log;
        _mediator = mediator;
        _downloadFileSystem = downloadFileSystem;
        DownloadWorkerTask = downloadWorkerTask;

        var options = new RestClientOptions()
        {
            MaxTimeout = 10000,
            ThrowOnAnyError = false,
        };

        _httpClient = new RestClient(httpClientFactory.CreateClient(), options);

        _timer.Elapsed += (_, _) => { DownloadWorkerTask.ElapsedTime += (long)_timer.Interval; };
    }

    #endregion

    #region Properties

    public Task<Result> DownloadProcessTask { get; internal set; }

    public IObservable<DownloadWorkerLog> DownloadWorkerLog => _downloadWorkerLog.AsObservable();

    public IObservable<DownloadWorkerTask> DownloadWorkerTaskUpdate => _downloadWorkerUpdate.AsObservable();

    /// <summary>
    /// Gets the current <see cref="DownloadWorkerTask"/> being executed.
    /// </summary>
    public DownloadWorkerTask DownloadWorkerTask { get; }

    public string FileName => DownloadWorkerTask.FileName;

    /// <summary>
    /// The download worker id, which is the same as the <see cref="DownloadWorkerTask"/> Id.
    /// </summary>
    public int Id => DownloadWorkerTask.Id;

    #endregion

    #region Public Methods

    public Result Start()
    {
        _log.Here().Debug("Download worker with id: {Id} start for filename: {FileName}", Id, FileName);

        DownloadProcessTask = DownloadProcessAsync();

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
        _downloadWorkerLog.OnCompleted();
        _downloadWorkerUpdate.OnCompleted();
        return Result.Ok(DownloadWorkerTask);
    }

    public void SetDownloadSpeedLimit(int speedInKb)
    {
        _downloadSpeedLimit = speedInKb;
    }

    public void Dispose()
    {
        _timer.Dispose();
        _httpClient?.Dispose();
        _downloadWorkerUpdate.Dispose();
        _downloadWorkerLog.Dispose();
    }

    #endregion

    #region Private Methods

    private async Task<Result> DownloadProcessAsync(CancellationToken cancellationToken = default)
    {
        ThrottledStream throttledStream = null;
        Stream destinationStream = null;
        Stream responseStream = null;
        try
        {
            var _fileStreamResult =
                _downloadFileSystem.CreateDownloadFileStream(DownloadWorkerTask.TempDirectory, FileName, DownloadWorkerTask.DataTotal);
            if (_fileStreamResult.IsFailed)
            {
                SendDownloadWorkerError(_fileStreamResult.ToResult());
                return _fileStreamResult.ToResult();
            }

            destinationStream = _fileStreamResult.Value;

            var plexServerConnectionResult =
                await _mediator.Send(new GetPlexServerConnectionByPlexServerIdQuery(DownloadWorkerTask.PlexServerId), cancellationToken);
            if (plexServerConnectionResult.IsFailed)
            {
                _log.Error("Could not find a valid connection to use for DownloadWorker with id {ID}", DownloadWorkerTask.Id);
                return plexServerConnectionResult.ToResult().LogError();
            }

            var plexServerConnection = plexServerConnectionResult.Value;
            var plexServer = plexServerConnection.PlexServer;

            var tokenResult = await _mediator.Send(new GetPlexServerTokenQuery(DownloadWorkerTask.PlexServerId), cancellationToken);
            if (tokenResult.IsFailed)
            {
                _log.Error("Could not find a valid token for server {ServerName} to use for DownloadWorker with id {ID}", plexServer.Name,
                    DownloadWorkerTask.Id);

                SendDownloadWorkerError(tokenResult.ToResult().LogError());
                return tokenResult.ToResult();
            }

            var downloadUrl = plexServerConnection.GetDownloadUrl(DownloadWorkerTask.FileLocationUrl, tokenResult.Value);

            // Is 0 when starting new and > 0 when resuming.
            destinationStream.Position = DownloadWorkerTask.BytesReceived;

            // Create download client
            var request = new RestRequest(downloadUrl)
            {
                CompletionOption = HttpCompletionOption.ResponseHeadersRead,
            };

            request.AddHeader("Range", new RangeHeaderValue(DownloadWorkerTask.CurrentByte, DownloadWorkerTask.EndByte).ToString());

            responseStream = await _httpClient.DownloadStreamAsync(request, cancellationToken);

            // Throttle the stream to enable download speed limiting
            throttledStream = new ThrottledStream(responseStream, _downloadSpeedLimit);

            // Buffer is based on: https://stackoverflow.com/a/39355385/8205497
            var buffer = new byte[(long)ByteSize.FromMebiBytes(4).Bytes];

            _timer.Start();

            SetDownloadWorkerTaskChanged(DownloadStatus.Downloading);

            while (_isDownloading)
            {
                throttledStream.SetThrottleSpeed(_downloadSpeedLimit);
                var bytesRead = await throttledStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                if (bytesRead > 0)
                    bytesRead = (int)Math.Min(DownloadWorkerTask.DataRemaining, bytesRead);

                if (bytesRead <= 0)
                {
                    await DisposeResources();
                    SendDownloadFinished();
                    break;
                }

                await destinationStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                await destinationStream.FlushAsync(cancellationToken);

                DownloadWorkerTask.BytesReceived += bytesRead;
                SendDownloadWorkerUpdate();
            }
        }
        catch (Exception e)
        {
            var result = Result.Fail(new ExceptionalError(e)).LogError();
            SendDownloadWorkerError(result);
            return result;
        }
        finally
        {
            await DisposeResources();
        }

        return Result.Ok();

        // Dispose all streams
        async Task DisposeResources()
        {
            if (responseStream != null)
                await responseStream.DisposeAsync();

            if (throttledStream != null)
                await throttledStream.DisposeAsync();

            if (destinationStream != null)
                await destinationStream.DisposeAsync();
        }
    }

    private void SendDownloadWorkerError(Result errorResult)
    {
        if (errorResult.Errors.Any() && !errorResult.Errors[0].Metadata.ContainsKey(nameof(DownloadWorker) + "Id"))
            errorResult.Errors[0].Metadata.Add(nameof(DownloadWorker) + "Id", Id);

        _log.Here().Error("Download worker {Id} with {FileName} had an error!", Id, FileName);
        DownloadWorkerTask.DownloadStatus = DownloadStatus.Error;
        SendDownloadWorkerUpdate();
        SendDownloadWorkerLog(NotificationLevel.Error, errorResult.ToString());
    }

    private void SendDownloadFinished()
    {
        _timer.Stop();

        SetDownloadWorkerTaskChanged(DownloadStatus.DownloadFinished);
        _downloadWorkerLog.OnCompleted();
        _downloadWorkerUpdate.OnCompleted();
        _log.Here().Information("Download worker {Id} with {FileName} finished!", Id, FileName);
    }

    private void SendDownloadWorkerLog(NotificationLevel logLevel, string message)
    {
        _downloadWorkerLog.OnNext(new DownloadWorkerLog
        {
            Message = message,
            LogLevel = logLevel,
            CreatedAt = DateTime.UtcNow,
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

        _log.Debug("Download worker with id: {Id} and with filename: {FileName} changed status to {Status}", Id, FileName, status);
        DownloadWorkerTask.DownloadStatus = status;
        SendDownloadWorkerLog(NotificationLevel.Information,
            $"Download worker with id: {Id} and with filename: {FileName} changed status to {status}");
        SendDownloadWorkerUpdate();
    }

    #endregion
}