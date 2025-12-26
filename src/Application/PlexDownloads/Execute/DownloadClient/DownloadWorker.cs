using System.Diagnostics;
using System.Net.Http.Headers;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using ByteSizeLib;
using Polly;
using Polly.Retry;
using Reaparr.Data.Contracts;
using Reaparr.PlexApi.Contracts;

namespace Reaparr.Application;

/// <summary>
/// The <see cref="DownloadWorker"/> is part of the multithreaded <see cref="PlexDownloadClient"/>
/// and will download a part of the <see cref="DownloadTaskGeneric"/>.
/// </summary>
public class DownloadWorker : IDisposable
{
    private readonly Subject<DownloadWorkerLog> _downloadWorkerLog = new();

    private readonly Subject<DownloadWorkerTaskProgress> _downloadWorkerUpdate = new();

    private readonly ILogger _log;

    private readonly ICommandExecutor _commandExecutor;

    private readonly IReaparrDbContext _dbContext;

    private readonly IPlexApiClient _httpClient;

    private int _downloadSpeedLimit;

    private readonly AsyncRetryPolicy<Result<int>> _retryPolicy;

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="DownloadWorker"/> class.
    /// </summary>
    /// <param name="log"></param>
    /// <param name="commandExecutor"></param>
    /// <param name="dbContext"></param>
    /// <param name="downloadWorkerTask">The download task this worker will execute.</param>
    /// <param name="clientFactory">The factory to create a new <see cref="IPlexApiClient"/>.</param>
    public DownloadWorker(
        ILogger log,
        ICommandExecutor commandExecutor,
        IReaparrDbContext dbContext,
        DownloadWorkerTask downloadWorkerTask,
        Func<PlexApiClientOptions?, IPlexApiClient> clientFactory
    )
    {
        _log = log.ForContext<DownloadWorker>();
        _commandExecutor = commandExecutor;
        _dbContext = dbContext;
        DownloadWorkerTask = downloadWorkerTask;

        _httpClient = clientFactory(
            new PlexApiClientOptions
            {
                Timeout = 20,
                RetryCount = 3,
                ConnectionUrl = string.Empty,
            }
        );

        _retryPolicy = Policy
            .Handle<HttpIOException>()
            .OrResult<Result<int>>(result => result.IsFailed)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                onRetry: (exception, timeSpan, retryCount, _) =>
                {
                    _log.Here()
                        .Warning(
                            "Retry {retryCount} due to {exceptionMessage}. Retrying in {timeSpanTotalSeconds} seconds.",
                            retryCount,
                            exception.Result?.Errors.FirstOrDefault()?.Message ?? "Unknown error",
                            timeSpan.TotalSeconds
                        );
                }
            );
    }

    public Task DownloadProcessTask { get; private set; } = Task.CompletedTask;

    public IObservable<DownloadWorkerLog> DownloadWorkerLog => _downloadWorkerLog.AsObservable();

    public IObservable<DownloadWorkerTaskProgress> DownloadWorkerTaskUpdate => _downloadWorkerUpdate.AsObservable();

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

        DownloadProcessTask = DownloadProcessAsync(_cancellationTokenSource.Token);

        return Result.Ok();
    }

    /// <summary>
    /// Stops the downloading.
    /// </summary>
    /// <returns>Is successful.</returns>
    public async Task<Result<DownloadWorkerTask>> StopAsync()
    {
        await _cancellationTokenSource.CancelAsync();

        // Wait for it to gracefully end.
        await DownloadProcessTask;

        return Result.Ok(DownloadWorkerTask);
    }

    public void SetDownloadSpeedLimit(int speedInKb)
    {
        _downloadSpeedLimit = speedInKb;
    }

    private async Task DownloadProcessAsync(CancellationToken cancellationToken)
    {
        Stream? destinationStream = null;
        ThrottledStream? responseStream = null;
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
                SetDownloadWorkerTaskChanged(DownloadStatus.ServerUnreachable);
                return;
            }

            var downloadUrl = downloadUrlResult.Value;

            // Prepare destination stream
            var fileStreamResult = await _commandExecutor.Send(
                new CreateDownloadFileStreamCommand(
                    DownloadWorkerTask.DownloadDirectory,
                    FileName,
                    DownloadWorkerTask.DataTotal
                ),
                CancellationToken.None
            );
            if (fileStreamResult.IsFailed)
            {
                var result = _log.Here()
                    .ErrorResult(
                        "Could not create a download destination filestream for DownloadWorker with id: {Id}",
                        Id
                    );

                SetDownloadWorkerTaskChanged(DownloadStatus.Error, Result.Merge(result, fileStreamResult).ToResult());
                return;
            }

            destinationStream = fileStreamResult.Value;

            // Position the destination stream at the absolute byte offset for this segment
            // Support resume by advancing with BytesReceived from the segment start
            destinationStream.Position = DownloadWorkerTask.StartByte + DownloadWorkerTask.BytesReceived;

            // Create download HttpRequestMessage with range header
            var request = new HttpRequestMessage(HttpMethod.Get, downloadUrl);
            request.Headers.Add(
                "Range",
                new RangeHeaderValue(DownloadWorkerTask.CurrentByte, DownloadWorkerTask.EndByte).ToString()
            );

            // Buffer is based on: https://stackoverflow.com/a/39355385/8205497
            var buffer = new byte[(long)ByteSize.FromMebiBytes(4).Bytes];

            var loopIndex = 0;
            var emptyStreamResponse = 0;
            var stopwatch = Stopwatch.StartNew(); // Start timing for speed calculation
            var previousBytesReceived = 0; // Track bytes received at the last speed calculation

            while (true)
            {
                var result = await _retryPolicy.ExecuteAsync(async () =>
                {
                    // Download the data
                    responseStream ??= await _httpClient.DownloadStreamAsync(
                        request,
                        _downloadSpeedLimit,
                        cancellationToken
                    );

                    if (responseStream is null)
                    {
                        return _log.Here()
                            .ErrorResult(
                                "Download worker {Id} with {FileName} had an empty download stream",
                                Id,
                                FileName
                            );
                    }

                    responseStream.SetThrottleSpeed(_downloadSpeedLimit);

                    var readResult = await Result.Try(() =>
                        responseStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)
                    );

                    if (readResult.IsFailed)
                    {
                        return readResult;
                    }

                    var read = readResult.Value;

                    // Clamp the read to the remaining data
                    if (read > 0)
                    {
                        SetDownloadWorkerTaskChanged(DownloadStatus.Downloading);
                        read = (int)Math.Min(DownloadWorkerTask.DataRemaining, read);
                    }

                    return Result.Ok(read);
                });

                if (result.IsFailed)
                {
                    if (result.Errors.Any(x => x.Message.Contains("A task was canceled.")))
                    {
                        SetDownloadWorkerTaskChanged(DownloadStatus.Stopped, result.ToResult());
                    }
                    else if (result.Errors.Any(x => x.Message.Contains("The response ended prematurely")))
                    {
                        SetDownloadWorkerTaskChanged(DownloadStatus.ServerUnreachable, result.ToResult());
                    }
                    else
                    {
                        SetDownloadWorkerTaskChanged(DownloadStatus.Error, result.ToResult());
                    }

                    break;
                }

                var bytesRead = result.Value;

                if (bytesRead == 0)
                {
                    emptyStreamResponse++;
                    if (emptyStreamResponse > 5)
                    {
                        SetDownloadWorkerTaskChanged(DownloadStatus.ServerUnreachable);
                        break;
                    }
                }

                if (loopIndex == 0 && bytesRead <= 0)
                {
                    var errorResult = _log.Here()
                        .ErrorResult(
                            "Download worker with id: {Id} and filename: {FileName} had and empty download stream on start",
                            Id,
                            FileName
                        );
                    SetDownloadWorkerTaskChanged(DownloadStatus.ServerUnreachable, errorResult);
                    break;
                }

                await destinationStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                await destinationStream.FlushAsync(cancellationToken);

                loopIndex++;
                DownloadWorkerTask.BytesReceived += bytesRead;
                previousBytesReceived += bytesRead;

                // Calculate the speed based on the elapsed time and bytes downloaded since last check
                if (stopwatch.ElapsedMilliseconds > 1000)
                {
                    DownloadWorkerTask.ElapsedTime += 1;

                    // Bytes per second
                    DownloadWorkerTask.DownloadSpeed = DataFormat.GetTransferSpeed(
                        previousBytesReceived,
                        stopwatch.Elapsed.TotalSeconds
                    );

                    previousBytesReceived = 0;
                    stopwatch.Restart();
                }

                if (DownloadWorkerTask.IsCompleted)
                {
                    _log.Here()
                        .Information(
                            "Download worker {WorkerId} completed downloading segment {FileName} (part {PartIndex})",
                            Id,
                            FileName,
                            DownloadWorkerTask.PartIndex
                        );
                    SetDownloadWorkerTaskChanged(DownloadStatus.DownloadFinished);
                    break;
                }

                SendDownloadWorkerUpdate();
            }
        }
        catch (OperationCanceledException)
        {
            SetDownloadWorkerTaskChanged(DownloadStatus.Stopped);
            _log.Here().Debug("Download worker with id: {Id} and filename: {FileName} was stopped", Id, FileName);
        }
        catch (Exception e)
        {
            SetDownloadWorkerTaskChanged(DownloadStatus.Error, Result.Fail(new ExceptionalError(e)));
        }
        finally
        {
            if (responseStream != null)
                await responseStream.DisposeAsync();

            if (destinationStream != null)
                await destinationStream.DisposeAsync();

            _downloadWorkerLog.OnCompleted();
            _downloadWorkerUpdate.OnCompleted();
        }
    }

    private void SetDownloadWorkerTaskChanged(DownloadStatus status, Result? errorResult = null)
    {
        if (DownloadWorkerTask.DownloadStatus == status)
            return;

        var msg = _log.Here()
            .DebugMsg(
                "Download worker with id: {Id} and with filename: {FileName} changed status to {Status}",
                Id,
                FileName,
                status
            );
        DownloadWorkerTask.DownloadStatus = status;

        SendDownloadWorkerLog(status.ToNotificationLevel(), msg);

        string? logMsg = null;
        switch (status)
        {
            case DownloadStatus.Stopped:
                logMsg = _log.Here().InformationMsg("Download worker {Id} with {FileName} was stopped!", Id, FileName);
                break;
            case DownloadStatus.Error:
                logMsg = _log.Here().ErrorMsg("Download worker {Id} with {FileName} had an error!", Id, FileName);
                break;
            case DownloadStatus.DownloadFinished:
                logMsg = _log.Here().InformationMsg("Download worker {Id} with {FileName} finished!", Id, FileName);
                break;
            case DownloadStatus.ServerUnreachable:
                logMsg = _log.Here()
                    .ErrorMsg(
                        "The server {PlexServerName} is unreachable!",
                        DownloadWorkerTask.PlexServer?.Name ?? "Unknown"
                    );
                break;
        }

        if (logMsg != null)
        {
            SendDownloadWorkerLog(status.ToNotificationLevel(), logMsg);
        }

        if (errorResult != null)
        {
            if (errorResult.Errors.Any() && !errorResult.Errors[0].Metadata.ContainsKey(nameof(DownloadWorker) + "Id"))
                errorResult.Errors[0].Metadata.Add(nameof(DownloadWorker) + "Id", Id);

            SendDownloadWorkerLog(NotificationLevel.Error, errorResult.ToString());
        }

        SendDownloadWorkerUpdate();
    }

    private void SendDownloadWorkerUpdate()
    {
        _downloadWorkerUpdate.OnNext(
            new DownloadWorkerTaskProgress
            {
                Id = DownloadWorkerTask.Id,
                DataTotal = DownloadWorkerTask.DataTotal,
                Percentage = DownloadWorkerTask.Percentage,
                DataReceived = DownloadWorkerTask.BytesReceived,
                ElapsedTime = DownloadWorkerTask.ElapsedTime,
                Status = DownloadWorkerTask.DownloadStatus,
                DownloadSpeed = DownloadWorkerTask.DownloadSpeed,
            }
        );
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

    public void Dispose()
    {
        _httpClient.Dispose();
        _cancellationTokenSource.Dispose();
        _downloadWorkerLog.Dispose();
        _downloadWorkerUpdate.Dispose();
    }
}
