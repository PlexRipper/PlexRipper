using System.IO.Abstractions;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;
using Reaparr.External.Contracts;
using Reaparr.Settings.Contracts;

namespace Reaparr.Application;

/// <summary>
/// Download client implementation that uses dash-mpd-cli for downloading DASH (MPEG-DASH) streaming content.
/// Unlike <see cref="PlexDownloadClient"/>, this implementation delegates the entire download process
/// to the dash-mpd-cli binary, which handles multi-threading, segment downloading, and muxing internally.
/// </summary>
public class DashPlexDownloadClient : IPlexDownloadClient
{
    private readonly ILogger _log;
    private readonly IReaparrDbContextFactory _dbContextFactory;
    private readonly IDashMpdCliWrapper _dashWrapper;
    private readonly ICommandExecutor _commandExecutor;
    private readonly IServerSettingsModule _serverSettings;
    private readonly IDirectory _directory;

    private readonly Subject<IList<DownloadWorkerLog>> _downloadWorkerLogSubject = new();
    private readonly TaskCompletionSource<object> _downloadProcessCompletionSource = new();
    private readonly TaskCompletionSource<object> _progressSubscriptionCompletionSource = new();
    private readonly TaskCompletionSource<object> _logSubscriptionCompletionSource = new();
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private IDisposable? _downloadSpeedLimitSubscription;
    private IDisposable? _progressSubscription;
    private IDisposable? _logSubscription;

    private string? _downloadUrl;

    private DownloadTaskKey _downloadTaskKey = new DownloadTaskKey
    {
        Type = DownloadTaskType.None,
        Id = Guid.Empty,
        PlexServerId = 0,
        PlexLibraryId = 0,
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="DashPlexDownloadClient"/> class.
    /// </summary>
    public DashPlexDownloadClient(
        ILogger log,
        IReaparrDbContextFactory dbContextFactory,
        IDashMpdCliWrapper dashWrapper,
        ICommandExecutor commandExecutor,
        IServerSettingsModule serverSettings,
        IDirectory directory
    )
    {
        _log = log.ForContext<DashPlexDownloadClient>();
        _dbContextFactory = dbContextFactory;
        _dashWrapper = dashWrapper;
        _commandExecutor = commandExecutor;
        _serverSettings = serverSettings;
        _directory = directory;
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

    public IObservable<IList<DownloadWorkerLog>> ListenToDownloadWorkerLog { get; private set; } =
        Observable.Empty<IList<DownloadWorkerLog>>();

    public Task DownloadProcessTask =>
        Task.WhenAll(
            _dashWrapper.ProcessExitTask,
            _downloadProcessCompletionSource.Task,
            _progressSubscriptionCompletionSource.Task,
            _logSubscriptionCompletionSource.Task
        );

    /// <summary>
    /// Setup this DashPlexDownloadClient to prepare for the download process.
    /// </summary>
    public async Task<Result> Setup(DownloadTaskKey downloadTaskKey, CancellationToken cancellationToken = default)
    {
        using var dbContext = _dbContextFactory.Create();
        var downloadTask = await dbContext.GetDownloadTaskAsync(downloadTaskKey, cancellationToken);
        if (downloadTask is null)
        {
            return ResultExtensions
                .EntityNotFound(nameof(DownloadTaskGeneric), downloadTaskKey.ToString())
                .LogWarning();
        }

        DownloadTask = downloadTask;
        _downloadTaskKey = downloadTaskKey;

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

        // Ensure the download directory exists
        var createDirectoryResult = Result.Try(() => _directory.CreateDirectory(DownloadTask.DownloadDirectory));
        if (createDirectoryResult.IsFailed)
            return createDirectoryResult.ToResult();

        // Set up a download speed limit watcher
        await SetupDownloadLimitWatcher(DownloadTask);

        // Set up subscriptions to observables
        SetupSubscriptions();

        // Set up process exit handling
        _ = MonitorProcessExitAsync();

        _log.Here().Debug("DashPlexDownloadClient setup completed for {FileName}", DownloadTask.FileName);

        return Result.Ok();
    }

    /// <summary>
    /// Starts the download process using dash-mpd-cli.
    /// </summary>
    public async Task<Result> Start(DownloadTaskKey downloadTaskKey, CancellationToken cancellationToken = default)
    {
        if (DownloadTask is null)
            return Result.Fail("The DashPlexDownloadClient has not been setup yet.").LogError();

        if (string.IsNullOrWhiteSpace(_downloadUrl))
            return Result.Fail("Download URL is not available.").LogError();

        _log.Here().Debug("Starting DASH download for {MediaFileName}", DownloadTask.FileName);

        // Ensure working directory exists immediately before launching the process
        var ensureDirResult = Result.Try(() => _directory.CreateDirectory(DownloadTask.DownloadDirectory));
        if (ensureDirResult.IsFailed)
        {
            _log.Here()
                .Error(
                    "Cannot create download directory {DownloadDirectory} for {FileName}. "
                        + "Ensure the volume is mounted and the path is accessible.",
                    DownloadTask.DownloadDirectory,
                    DownloadTask.FileName
                );
            return ensureDirResult.ToResult();
        }

        // Configure dash-mpd-cli options
        var options = new DashMpdCliOptions
        {
            MpdUrl = _downloadUrl,
            Output = Path.Combine(DownloadTask.DownloadDirectory, DownloadTask.FileName),
            WorkingDirectory = DownloadTask.DownloadDirectory,
            Quiet = false,
            Quality = "best",
            EnvironmentVariables = new Dictionary<string, string>
            {
                ["TMPDIR"] = DownloadTask.DownloadDirectory,
                ["TMP"] = DownloadTask.DownloadDirectory,
            },
        };

        // Start the download process
        var startedResult = await _dashWrapper.StartAsync(options);
        if (startedResult.IsFailed)
            return startedResult;

        // Update status to downloading
        DownloadStatus = DownloadStatus.Downloading;
        using var dbContext = await _dbContextFactory.CreateAsync();
        await dbContext.SetDownloadStatus(DownloadTask.ToKey(), DownloadStatus);

        return Result.Ok();
    }

    /// <summary>
    /// Sets up subscriptions to the dash-mpd-cli wrapper observables.
    /// </summary>
    private void SetupSubscriptions()
    {
        _log.Here().Debug("Setting up observable subscriptions");

        // Progress subscription with error handling and completion tracking
        _progressSubscription = _dashWrapper
            .Progress.Sample(TimeSpan.FromMilliseconds(500))
            .Select(data => Observable.FromAsync(() => OnProgressUpdate(data)))
            .Concat()
            .Subscribe(
                _ => { },
                onError: ex =>
                {
                    if (ex is not OperationCanceledException)
                    {
                        _log.Here().ErrorResult(ex);
                        _progressSubscriptionCompletionSource.TrySetException(ex);
                    }
                    else
                    {
                        _log.Here().Debug("Progress subscription cancelled");
                        _progressSubscriptionCompletionSource.TrySetResult(true);
                    }
                },
                onCompleted: () =>
                {
                    _log.Here().Debug("Progress observable completed");
                    _progressSubscriptionCompletionSource.TrySetResult(true);
                }
            );

        // Log subscription - combine stdout and stderr with buffering
        ListenToDownloadWorkerLog = _dashWrapper
            .StandardOutput.Select(line => CreateLogEntry(line, NotificationLevel.Information))
            .Buffer(TimeSpan.FromSeconds(1))
            .Where(logs => logs.Any())
            .Select(logs => (IList<DownloadWorkerLog>)logs.ToList())
            .AsObservable();

        _logSubscription = ListenToDownloadWorkerLog.Subscribe(
            onNext: logs => _downloadWorkerLogSubject.OnNext(logs),
            onError: ex =>
            {
                if (ex is not OperationCanceledException)
                {
                    _log.Here().ErrorResult(ex);
                    _logSubscriptionCompletionSource.TrySetException(ex);
                }
                else
                {
                    _log.Here().Debug("Log subscription cancelled");
                    _logSubscriptionCompletionSource.TrySetResult(true);
                }
            },
            onCompleted: () =>
            {
                _log.Here().Debug("Log observable completed");
                _logSubscriptionCompletionSource.TrySetResult(true);
            }
        );

        _log.Here().Information("Observable subscriptions setup completed");
    }

    /// <summary>
    /// Creates a download worker log entry from a raw output line.
    /// </summary>
    private DownloadWorkerLog CreateLogEntry(string data, NotificationLevel level) =>
        new()
        {
            CreatedAt = DateTime.UtcNow,
            Message = level == NotificationLevel.Error ? $"ERROR: {data}" : data,
            DownloadTaskId = DownloadTask?.Id ?? Guid.Empty,
            LogLevel = level,
            Status = DownloadStatus,
        };

    /// <summary>
    /// Stops the download process.
    /// </summary>
    public async Task<Result> StopAsync()
    {
        _log.Here().Information("Stopping DASH download for {DownloadTaskFileName}", DownloadTask?.FileName);

        try
        {
            await _cancellationTokenSource.CancelAsync();

            await _dashWrapper.StopAsync();

            if (DownloadTask != null)
            {
                DownloadStatus = DownloadStatus.Paused;
                using var dbContext = _dbContextFactory.Create();
                await dbContext.SetDownloadStatus(_downloadTaskKey, DownloadStatus);
                await _commandExecutor.Send(new DownloadTaskUpdatedCommand(_downloadTaskKey));
            }

            // Wait for all tasks to complete
            await Task.WhenAll(_progressSubscriptionCompletionSource.Task, _logSubscriptionCompletionSource.Task);

            // Complete observables
            _downloadWorkerLogSubject.OnCompleted();
            _downloadProcessCompletionSource.TrySetResult(true);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _log.Here().ErrorResult(ex);
            return Result.Fail(new ExceptionalError("Failed to stop download", ex));
        }
    }

    private async Task OnProgressUpdate(DashDownloadProgress progress)
    {
        if (DownloadTask is null)
        {
            return;
        }

        DownloadTask.DownloadSpeed = progress.DownloadSpeedInBytes;
        DownloadTask.DataReceived = progress.DownloadedBytes;
        DownloadTask.DataTotal =
            progress.Percent > 0 ? progress.DownloadedBytes * 100 / progress.Percent : progress.TotalBytes;

        try
        {
            using var dbContext = _dbContextFactory.Create();
            await dbContext.UpdateDownloadProgress(_downloadTaskKey, DownloadTask!);
            await _commandExecutor.Send(new DownloadTaskUpdatedCommand(_downloadTaskKey));

            _log.Here()
                .Debug(
                    "Progress: {Percent}% - {DownloadSpeed} MB/s - {DataReceived} / {DataTotal} ",
                    DownloadTask.Percentage,
                    DownloadTask.DownloadSpeed.ToMebibytes(),
                    DownloadTask.DataReceived,
                    DownloadTask.DataTotal
                );
        }
        catch (Exception ex)
        {
            _log.Here().Warning(ex, "Failed to update download progress");
        }
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

            using var dbContext = await _dbContextFactory.CreateAsync();
            if (exitCode == 0)
            {
                // Download completed successfully - set DownloadFinished so DownloadJobListener
                // triggers the file move pipeline before transitioning to Completed
                DownloadStatus = DownloadStatus.DownloadFinished;
                await dbContext.SetDownloadStatus(_downloadTaskKey, DownloadStatus);
            }
            else
            {
                // Download failed
                DownloadStatus = DownloadStatus.Error;
                await dbContext.SetDownloadStatus(_downloadTaskKey, DownloadStatus);

                _log.Here()
                    .Error("Download failed for {FileName} with exit code {ExitCode}", DownloadTask.FileName, exitCode);
            }

            await _commandExecutor.Send(new DownloadTaskUpdatedCommand(_downloadTaskKey));

            // Complete the download process
            _downloadWorkerLogSubject.OnCompleted();
            _downloadProcessCompletionSource.TrySetResult(true);
        }
        catch (Exception ex)
        {
            _log.Here().ErrorResult(ex);
            _downloadProcessCompletionSource.TrySetException(ex);
        }
    }

    private async Task SetupDownloadLimitWatcher(DownloadTaskGeneric downloadTask)
    {
        using var dbContext = _dbContextFactory.Create();
        var serverMachineIdentifier = await dbContext.GetPlexServerMachineIdentifierById(downloadTask.PlexServerId);

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

    public void Dispose() { }

    /// <summary>
    /// Disposes of resources.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        _log.Here().Debug("Disposing DashPlexDownloadClient for {DownloadTaskId}", DownloadTask?.Id);

        await _dashWrapper.StopAsync();

        // Wait for download process to complete
        await DownloadProcessTask;

        // Dispose subscriptions
        _progressSubscription?.Dispose();
        _logSubscription?.Dispose();
        _downloadSpeedLimitSubscription?.Dispose();

        // Dispose subjects
        _downloadWorkerLogSubject.Dispose();

        await _dashWrapper.DisposeAsync();
        _cancellationTokenSource.Dispose();

        _log.Here()
            .Warning(
                "DashPlexDownloadClient for DownloadTask with Id: {DownloadTaskId} was disposed",
                DownloadTask?.Id
            );
    }
}
