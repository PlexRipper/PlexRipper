using System.Reactive.Linq;
using System.Reactive.Subjects;
using Flurl;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;
using Reaparr.External.Contracts;
using Reaparr.Settings.Contracts;
using DataReceivedEventArgs = System.Diagnostics.DataReceivedEventArgs;

namespace Reaparr.Application;

/// <summary>
/// Download client implementation that uses dash-mpd-cli for downloading DASH (MPEG-DASH) streaming content.
/// Unlike <see cref="PlexDownloadClient"/>, this implementation delegates the entire download process
/// to the dash-mpd-cli binary, which handles multi-threading, segment downloading, and muxing internally.
/// </summary>
public class DashPlexDownloadClient : IPlexDownloadClient
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly IDashMpdCliWrapper _dashWrapper;
    private readonly ICommandExecutor _commandExecutor;
    private readonly IServerSettingsModule _serverSettings;

    private readonly Subject<IList<DownloadWorkerLog>> _downloadWorkerLog = new();
    private readonly TaskCompletionSource<object> _downloadProcessCompletion = new();
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private IDisposable? _downloadSpeedLimitSubscription;
    private string? _downloadUrl;
    private string? _outputPath;

    private long _lastBytesDownloaded;
    private long _lastTotalBytes;
    private long _lastSpeed;
    private DateTime _lastProgressUpdate = DateTime.UtcNow;

    /// <summary>
    /// Initializes a new instance of the <see cref="DashPlexDownloadClient"/> class.
    /// </summary>
    public DashPlexDownloadClient(
        ILogger log,
        IReaparrDbContext dbContext,
        IDashMpdCliWrapper dashWrapper,
        ICommandExecutor commandExecutor,
        IServerSettingsModule serverSettings
    )
    {
        _log = log.ForContext<DashPlexDownloadClient>();
        _dbContext = dbContext;
        _dashWrapper = dashWrapper;
        _commandExecutor = commandExecutor;
        _serverSettings = serverSettings;
    }

    /// <summary>
    /// Gets the current <see cref="DownloadTaskGeneric"/> being executed.
    /// </summary>
    public DownloadTaskGeneric? DownloadTask { get; private set; }

    public DownloadStatus DownloadStatus
    {
        get => DownloadTask?.DownloadStatus ?? DownloadStatus.Unknown;
        private set
        {
            if (DownloadTask != null)
                DownloadTask.DownloadStatus = value;
        }
    }

    public IObservable<IList<DownloadWorkerLog>> ListenToDownloadWorkerLog => _downloadWorkerLog.AsObservable();

    public Task DownloadProcessTask => _downloadProcessCompletion.Task;

    /// <summary>
    /// Setup this DashPlexDownloadClient to prepare for the download process.
    /// </summary>
    public async Task<Result> Setup(DownloadTaskKey downloadTaskKey, CancellationToken cancellationToken = default)
    {
        var downloadTask = await _dbContext.GetDownloadTaskAsync(downloadTaskKey, cancellationToken);
        if (downloadTask is null)
        {
            return ResultExtensions
                .EntityNotFound(nameof(DownloadTaskGeneric), downloadTaskKey.ToString())
                .LogWarning();
        }

        DownloadTask = downloadTask;

        // Get the download URL from the Plex server
        var downloadUrlResult = await _commandExecutor.Send(
            new GetDashDownloadUrlCommand
            {
                DownloadTaskKey = downloadTaskKey,
                MetaDataPath = "/library/metadata/" + DownloadTask.RatingKey,
            },
            cancellationToken
        );

        if (downloadUrlResult.IsFailed)
            return downloadUrlResult.ToResult().LogError();

        _downloadUrl = downloadUrlResult.Value;

        _log.Here().Debug("DashPlexDownloadClient setup started for URL {DownloadURL}", _downloadUrl);

        // Build output path
        _outputPath = Path.Combine(DownloadTask.DownloadDirectory, DownloadTask.FileName);

        // Ensure the download directory exists
        var directoryResult = await _commandExecutor.Send(
            new CreateDownloadFileStreamCommand(DownloadTask.DownloadDirectory, DownloadTask.FileName, 0),
            cancellationToken
        );

        if (directoryResult.IsFailed)
        {
            return directoryResult.ToResult().LogError();
        }

        // Dispose the stream immediately - we just needed to create the directory
        await directoryResult.Value.DisposeAsync();

        // Wire up event handlers
        _dashWrapper.OutputDataReceived += OnOutputDataReceived;
        _dashWrapper.ErrorDataReceived += OnErrorDataReceived;
        _dashWrapper.ProgressUpdated += OnProgressUpdated;

        // Set up a download speed limit watcher
        await SetupDownloadLimitWatcher(DownloadTask);

        // Set up process exit handling
        _ = MonitorProcessExitAsync();

        _log.Here().Debug("DashPlexDownloadClient setup completed for {FileName}", DownloadTask.FileName);

        return Result.Ok();
    }

    /// <summary>
    /// Starts the download process using dash-mpd-cli.
    /// </summary>
    public Result Start()
    {
        if (DownloadTask is null)
        {
            return Result.Fail("The DashPlexDownloadClient has not been setup yet.").LogError();
        }

        if (_dashWrapper.IsRunning)
        {
            return Result.Fail("The DashPlexDownloadClient is already downloading and cannot be started.").LogWarning();
        }

        if (string.IsNullOrWhiteSpace(_downloadUrl))
        {
            return Result.Fail("Download URL is not available.").LogError();
        }

        if (string.IsNullOrWhiteSpace(_outputPath))
        {
            return Result.Fail("Output path is not configured.").LogError();
        }

        _log.Here().Debug("Starting DASH download for {MediaFileName}", DownloadTask.FileName);

        try
        {
            // Configure dash-mpd-cli options
            var options = new DashMpdCliOptions
            {
                Quiet = false, // We need output for progress tracking
                Verbose = true,
                WorkingDirectory = DownloadTask.DownloadDirectory,
                Quality = "best",
            };

            // Start the download process
            var started = _dashWrapper.Start(_downloadUrl, _outputPath, options);

            if (!started)
            {
                return Result.Fail("Failed to start dash-mpd-cli process.").LogError();
            }

            // Update status to downloading
            DownloadStatus = DownloadStatus.Downloading;
            _ = _dbContext.SetDownloadStatus(DownloadTask.ToKey(), DownloadStatus);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result
                .Fail(new ExceptionalError($"Could not start download for {DownloadTask.FileName}", ex))
                .LogError();
        }
    }

    /// <summary>
    /// Stops the download process.
    /// </summary>
    public async Task<Result> StopAsync()
    {
        _log.Here().Information("Stopping DASH download for {DownloadTaskFileName}", DownloadTask?.FileName);

        try
        {
            await _cancellationTokenSource.CancelAsync();

            if (_dashWrapper.IsRunning)
            {
                await _dashWrapper.StopAsync(TimeSpan.FromSeconds(10));
            }

            if (DownloadTask != null)
            {
                DownloadStatus = DownloadStatus.Paused;
                await _dbContext.SetDownloadStatus(DownloadTask.ToKey(), DownloadStatus);
                await _commandExecutor.Send(new DownloadTaskUpdatedCommand(DownloadTask.ToKey()));
            }

            // Complete observables
            _downloadWorkerLog.OnCompleted();
            _downloadProcessCompletion.TrySetResult(true);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _log.Here().ErrorResult(ex);
            return Result.Fail(new ExceptionalError("Failed to stop download", ex));
        }
    }

    /// <summary>
    /// Disposes of resources.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        _log.Here().Debug("Disposing DashPlexDownloadClient for {DownloadTaskId}", DownloadTask?.Id);

        // Unsubscribe from events
        _dashWrapper.OutputDataReceived -= OnOutputDataReceived;
        _dashWrapper.ErrorDataReceived -= OnErrorDataReceived;
        _dashWrapper.ProgressUpdated -= OnProgressUpdated;

        // Dispose subscriptions
        _downloadSpeedLimitSubscription?.Dispose();

        // Dispose subjects
        _downloadWorkerLog.Dispose();

        // Ensure process is stopped
        if (_dashWrapper.IsRunning)
        {
            await _dashWrapper.StopAsync();
        }

        await _dashWrapper.DisposeAsync();
        _cancellationTokenSource.Dispose();

        GC.SuppressFinalize(this);
    }

    private void OnOutputDataReceived(object? sender, DataReceivedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(e.Data))
        {
            _log.Here().Debug("[dash-mpd-cli] {Output}", e.Data);

            // Create log entry
            var logEntry = new DownloadWorkerLog
            {
                DownloadWorkerTaskId = DownloadTask?.DownloadWorkerTasks.FirstOrDefault()?.Id ?? 0,
                CreatedAt = DateTime.UtcNow,
                Message = e.Data,
                DownloadTaskId = DownloadTask!.Id,
                LogLevel = NotificationLevel.Information,
            };

            _downloadWorkerLog.OnNext([logEntry]);
        }
    }

    private void OnErrorDataReceived(object? sender, DataReceivedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(e.Data))
        {
            _log.Here().Warning("[dash-mpd-cli ERROR] {Error}", e.Data);

            // Create log entry for errors
            var logEntry = new DownloadWorkerLog
            {
                DownloadWorkerTaskId = DownloadTask?.DownloadWorkerTasks.FirstOrDefault()?.Id ?? 0,
                CreatedAt = DateTime.UtcNow,
                Message = $"ERROR: {e.Data}",
                DownloadTaskId = DownloadTask!.Id,
                LogLevel = NotificationLevel.Error,
            };

            _downloadWorkerLog.OnNext([logEntry]);
        }
    }

    private void OnProgressUpdated(object? sender, DownloadProgressEventArgs e)
    {
        if (DownloadTask == null)
            return;

        // Update local tracking
        if (e.BytesDownloaded > 0)
            _lastBytesDownloaded = e.BytesDownloaded;

        if (e.TotalBytes > 0)
            _lastTotalBytes = e.TotalBytes;

        if (e.BytesPerSecond > 0)
            _lastSpeed = e.BytesPerSecond;

        // Throttle database updates to every 500ms
        if ((DateTime.UtcNow - _lastProgressUpdate).TotalMilliseconds < 500)
            return;

        _lastProgressUpdate = DateTime.UtcNow;

        // Update download task progress
        DownloadTask.DataReceived = _lastBytesDownloaded;
        if (_lastTotalBytes > 0)
            DownloadTask.DataTotal = _lastTotalBytes;
        DownloadTask.DownloadSpeed = _lastSpeed;

        // Update database asynchronously
        _ = Task.Run(async () =>
        {
            try
            {
                await _dbContext.UpdateDownloadProgress(DownloadTask.ToKey(), DownloadTask);
                await _commandExecutor.Send(new DownloadTaskUpdatedCommand(DownloadTask.ToKey()));

                _log.Here()
                    .Verbose(
                        "Progress: {Percent}% - {Downloaded}/{Total} - {Speed}/s",
                        e.PercentComplete,
                        DataFormat.FormatSizeString(_lastBytesDownloaded),
                        DataFormat.FormatSizeString(_lastTotalBytes),
                        DataFormat.FormatSpeedString(_lastSpeed)
                    );
            }
            catch (Exception ex)
            {
                _log.Here().Warning(ex, "Failed to update download progress");
            }
        });
    }

    private async Task MonitorProcessExitAsync()
    {
        try
        {
            var exitCode = await _dashWrapper.ProcessExitTask;

            if (DownloadTask == null)
                return;

            _log.Here()
                .Information(
                    "dash-mpd-cli process exited with code {ExitCode} for {DownloadFileName}",
                    exitCode,
                    DownloadTask.FileName
                );

            if (exitCode == 0)
            {
                // Download completed successfully
                DownloadStatus = DownloadStatus.Completed;
                await _dbContext.SetDownloadStatus(DownloadTask.ToKey(), DownloadStatus);
            }
            else
            {
                // Download failed
                DownloadStatus = DownloadStatus.Error;
                await _dbContext.SetDownloadStatus(DownloadTask.ToKey(), DownloadStatus);

                _log.Here()
                    .Error("Download failed for {FileName} with exit code {ExitCode}", DownloadTask.FileName, exitCode);
            }

            await _commandExecutor.Send(new DownloadTaskUpdatedCommand(DownloadTask.ToKey()));

            // Complete the download process
            _downloadWorkerLog.OnCompleted();
            _downloadProcessCompletion.TrySetResult(true);
        }
        catch (Exception ex)
        {
            _log.Here().ErrorResult(ex);
            _downloadProcessCompletion.TrySetException(ex);
        }
    }

    private async Task SetupDownloadLimitWatcher(DownloadTaskGeneric downloadTask)
    {
        var serverMachineIdentifier = await _dbContext.GetPlexServerMachineIdentifierById(downloadTask.PlexServerId);

        var downloadSpeedLimit = _serverSettings.GetDownloadSpeedLimit(serverMachineIdentifier);

        // Note: dash-mpd-cli uses --limit-rate option which we could set in options
        // For now, we'll just log it
        _log.Here()
            .Debug(
                "Download speed limit for server {Server}: {Limit} KB/s",
                serverMachineIdentifier,
                downloadSpeedLimit
            );

        _downloadSpeedLimitSubscription = _serverSettings
            .GetDownloadSpeedLimitObservable(serverMachineIdentifier)
            .Subscribe(limit =>
            {
                _log.Here().Debug("Download speed limit changed to {Limit} KB/s", limit);
                // TODO: Implement runtime speed limit changes if dash-mpd-cli supports it
            });
    }
}
