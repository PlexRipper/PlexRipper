using System.Net.Http.Headers;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using ByteSizeLib;
using Data.Contracts;
using FileSystem.Contracts;
using Logging.Interface;
using PlexApi.Contracts;
using Timer = System.Timers.Timer;

namespace PlexRipper.Application;

/// <summary>
/// The <see cref="DownloadWorker"/> is part of the multi-threaded <see cref="PlexDownloadClient"/>
/// and will download a part of the <see cref="DownloadTaskGeneric"/>.
/// </summary>
public class DownloadWorker : IDisposable
{
    private readonly Subject<DownloadWorkerLog> _downloadWorkerLog = new();

    private readonly Subject<DownloadWorkerTask> _downloadWorkerUpdate = new();

    private readonly ILog<DownloadWorker> _log;

    private readonly IDownloadFileStream _downloadFileSystem;
    private readonly IPlexRipperDbContext _dbContext;

    private readonly IPlexApiClient _httpClient;

    private readonly Timer _timer = new(100) { AutoReset = true };

    private int _downloadSpeedLimit;

    private bool _isDownloading = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="DownloadWorker"/> class.
    /// </summary>
    /// <param name="log"></param>
    /// <param name="dbContext"></param>
    /// <param name="downloadWorkerTask">The download task this worker will execute.</param>
    /// <param name="downloadFileSystem">The filesystem used to store the downloaded data.</param>
    public DownloadWorker(
        ILog<DownloadWorker> log,
        IPlexRipperDbContext dbContext,
        DownloadWorkerTask downloadWorkerTask,
        IDownloadFileStream downloadFileSystem,
        Func<PlexApiClientOptions?, IPlexApiClient> clientFactory
    )
    {
        _log = log;
        _downloadFileSystem = downloadFileSystem;
        _dbContext = dbContext;
        DownloadWorkerTask = downloadWorkerTask;

        _httpClient = clientFactory(new PlexApiClientOptions() { ConnectionUrl = string.Empty });

        _timer.Elapsed += (_, _) =>
        {
            DownloadWorkerTask.ElapsedTime += (long)_timer.Interval;
        };
    }

    public Task<Result> DownloadProcessTask { get; private set; } = new(Result.Ok);

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
        await DownloadProcessTask;

        SetDownloadWorkerTaskChanged(DownloadStatus.Stopped);
        Shutdown();
        return Result.Ok(DownloadWorkerTask);
    }

    public void SetDownloadSpeedLimit(int speedInKb)
    {
        _downloadSpeedLimit = speedInKb;
    }

    public void Dispose()
    {
        _timer.Dispose();
        _httpClient.Dispose();
        _downloadWorkerUpdate.Dispose();
        _downloadWorkerLog.Dispose();
    }

    private async Task<Result> DownloadProcessAsync(CancellationToken cancellationToken = default)
    {
        ThrottledStream? throttledStream = null;
        Stream? destinationStream = null;
        Stream? responseStream = null;
        try
        {
            // Retrieve Download URL
            var downloadUrlResult = await _dbContext.GetDownloadUrl(
                DownloadWorkerTask.PlexServerId,
                DownloadWorkerTask.FileLocationUrl,
                cancellationToken
            );

            if (downloadUrlResult.IsFailed)
            {
                SendDownloadWorkerError(downloadUrlResult.ToResult());
                return downloadUrlResult.ToResult();
            }

            var downloadUrl = downloadUrlResult.Value;

            // Prepare destination stream
            var fileStreamResult = _downloadFileSystem.CreateDownloadFileStream(
                DownloadWorkerTask.DownloadDirectory,
                FileName,
                DownloadWorkerTask.DataTotal
            );
            if (fileStreamResult.IsFailed)
            {
                var result = _log.Here()
                    .Error("Could not create a download destination filestream for DownloadWorker with id: {Id}", Id)
                    .ToResult();
                result.Errors.AddRange(fileStreamResult.Errors);
                SendDownloadWorkerError(result);
                return result;
            }

            destinationStream = fileStreamResult.Value;

            // Is 0 when starting new and > 0 when resuming.
            destinationStream.Position = DownloadWorkerTask.BytesReceived;

            // Create download client
            var request = new HttpRequestMessage(HttpMethod.Get, downloadUrl);
            request.Headers.Add(
                "Range",
                new RangeHeaderValue(DownloadWorkerTask.CurrentByte, DownloadWorkerTask.EndByte).ToString()
            );

            responseStream = await _httpClient.DownloadStreamAsync(request, cancellationToken);
            if (responseStream is null)
            {
                return SendDownloadWorkerError(Result.Fail(new ExceptionalError(new Exception(request.ToString()))));
            }

            // Throttle the stream to enable download speed limiting
            throttledStream = new ThrottledStream(responseStream, _downloadSpeedLimit);

            // Buffer is based on: https://stackoverflow.com/a/39355385/8205497
            var buffer = new byte[(long)ByteSize.FromMebiBytes(4).Bytes];

            _timer.Start();

            SetDownloadWorkerTaskChanged(DownloadStatus.Downloading);
            var loopIndex = 0;
            while (_isDownloading)
            {
                throttledStream.SetThrottleSpeed(_downloadSpeedLimit);
                var bytesRead = await throttledStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                if (bytesRead > 0)
                    bytesRead = (int)Math.Min(DownloadWorkerTask.DataRemaining, bytesRead);

                // We have at least downloaded something
                if (loopIndex > 0 && bytesRead <= 0)
                {
                    await DisposeResources();
                    SendDownloadFinished();
                    break;
                }

                if (loopIndex == 0 && bytesRead <= 0)
                {
                    SendDownloadWorkerError(
                        _log.Here()
                            .Error(
                                "Download worker with id: {Id} and filename: {FileName} had and empty download stream on start",
                                Id,
                                FileName
                            )
                            .ToResult()
                    );
                    break;
                }

                await destinationStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                await destinationStream.FlushAsync(cancellationToken);

                DownloadWorkerTask.BytesReceived += bytesRead;
                SendDownloadWorkerUpdate();
                loopIndex++;
            }
        }
        catch (Exception e)
        {
            return SendDownloadWorkerError(Result.Fail(new ExceptionalError(e)));
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

    private Result SendDownloadWorkerError(Result errorResult)
    {
        if (errorResult.Errors.Any() && !errorResult.Errors[0].Metadata.ContainsKey(nameof(DownloadWorker) + "Id"))
            errorResult.Errors[0].Metadata.Add(nameof(DownloadWorker) + "Id", Id);

        _log.Here().Error("Download worker {Id} with {FileName} had an error!", Id, FileName);
        errorResult.LogError();
        DownloadWorkerTask.DownloadStatus = DownloadStatus.Error;
        SendDownloadWorkerUpdate();
        SendDownloadWorkerLog(NotificationLevel.Error, errorResult.ToString());
        Shutdown();

        return errorResult;
    }

    private void SendDownloadFinished()
    {
        SetDownloadWorkerTaskChanged(DownloadStatus.DownloadFinished);
        _log.Here().Information("Download worker {Id} with {FileName} finished!", Id, FileName);
        Shutdown();
    }

    private void SendDownloadWorkerLog(NotificationLevel logLevel, string message)
    {
        _downloadWorkerLog.OnNext(
            new DownloadWorkerLog
            {
                Message = message,
                LogLevel = logLevel,
                CreatedAt = DateTime.UtcNow,
                DownloadWorkerTaskId = DownloadWorkerTask.Id,
                DownloadTaskId = DownloadWorkerTask.DownloadTaskId,
            }
        );
    }

    private void SendDownloadWorkerUpdate()
    {
        _downloadWorkerUpdate.OnNext(DownloadWorkerTask);
    }

    private void SetDownloadWorkerTaskChanged(DownloadStatus status)
    {
        if (DownloadWorkerTask.DownloadStatus == status)
            return;

        _log.Debug(
            "Download worker with id: {Id} and with filename: {FileName} changed status to {Status}",
            Id,
            FileName,
            status
        );
        DownloadWorkerTask.DownloadStatus = status;
        SendDownloadWorkerLog(
            NotificationLevel.Information,
            $"Download worker with id: {Id} and with filename: {FileName} changed status to {status}"
        );
        SendDownloadWorkerUpdate();
    }

    private void Shutdown()
    {
        _isDownloading = false;
        _timer.Stop();
        _downloadWorkerLog.OnCompleted();
        _downloadWorkerUpdate.OnCompleted();
    }
}
