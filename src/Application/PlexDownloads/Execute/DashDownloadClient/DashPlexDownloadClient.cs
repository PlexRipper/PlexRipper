using System.IO.Abstractions;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;
using Reaparr.External.Contracts;
using Reaparr.Settings.Contracts;

namespace Reaparr.Application;

/// <summary>
/// Download client implementation that uses dash-mpd-cli for downloading DASH (MPEG-DASH) streaming content.
/// Unlike <see cref="DirectPlexDownloadClient"/>, this implementation delegates the entire download process
/// to the dash-mpd-cli binary, which handles multi-threading, segment downloading, and muxing internally.
/// </summary>
public class DashPlexDownloadClient : IPlexDownloadClient
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly IReaparrDbContextFactory _dbContextFactory;
    private readonly IDashMpdCliWrapper _dashWrapper;
    private readonly ICommandExecutor _commandExecutor;
    private readonly IServerSettingsModule _serverSettings;
    private readonly IDirectory _directory;

    private DownloadTaskKey? _downloadTaskKey;
    private string _filename = string.Empty;

    private readonly CompositeDisposable _subscriptions = new();
    private readonly Subject<Unit> _destroy = new();
    private bool _disposed;

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
        _dbContext = dbContextFactory.Create();
    }

    public async Task<Result> Start(DownloadTaskKey downloadTaskKey, CancellationToken cancellationToken = default)
    {
        _downloadTaskKey = downloadTaskKey;

        var downloadTask = await _dbContext.GetDownloadTaskFileAsync(downloadTaskKey, cancellationToken);
        if (downloadTask is null)
        {
            return ResultExtensions
                .EntityNotFound(nameof(DownloadTaskGeneric), downloadTaskKey.ToString())
                .LogWarning();
        }

        _filename = downloadTask.FileName;

        // Get transcoding download url
        var downloadUrlResult = await _commandExecutor.Send(
            new GetDashDownloadUrlCommand
            {
                DownloadTaskKey = downloadTaskKey,
                MetaDataPath = $"/library/metadata/{downloadTask.PlexApiRatingKey}",
            },
            cancellationToken
        );

        if (downloadUrlResult.IsCancelled)
        {
            await SetDownloadStatusAsync(DownloadStatus.Stopped, downloadUrlResult.ToResult());
            return downloadUrlResult.ToResult();
        }

        if (downloadUrlResult.IsFailed)
            return downloadUrlResult.ToResult().LogError();

        // Create working directory
        var createDirectoryResult = Result.Try(() => _directory.CreateDirectory(downloadTask.DownloadDirectory));
        if (createDirectoryResult.IsFailed)
        {
            await SetDownloadStatusAsync(DownloadStatus.StorageError, createDirectoryResult.ToResult());
            return createDirectoryResult.ToResult();
        }

        await SetupDownloadListeners(downloadTaskKey);

        // Execute dash stream download
        await SetDownloadStatusAsync(DownloadStatus.Downloading);
        var options = await CreateDashOptions(downloadTask, downloadUrlResult.Value, cancellationToken);
        var startResult = await _dashWrapper.StartAsync(options);
        if (startResult.IsCancelled)
        {
            await SetDownloadStatusAsync(DownloadStatus.Stopped, downloadUrlResult.ToResult());
            return startResult;
        }

        if (startResult.IsFailed)
        {
            await SetDownloadStatusAsync(DownloadStatus.DownloadClientError, startResult);
            return startResult;
        }

        await SetDownloadStatusAsync(DownloadStatus.DownloadFinished);
        return Result.Ok();
    }

    public async Task<Result> StopAsync()
    {
        await _dashWrapper.StopAsync();

        if (_downloadTaskKey is null)
            return Result.Ok();

        await SetDownloadStatusAsync(DownloadStatus.Stopped);
        return Result.Ok();
    }

    private async Task<DashMpdCliOptions> CreateDashOptions(
        DownloadTaskFileBase downloadTask,
        string downloadUrl,
        CancellationToken cancellationToken
    )
    {
        var serverMachineIdentifier = await _dbContext.GetPlexServerMachineIdentifierById(
            downloadTask.PlexServerId,
            cancellationToken
        );
        var speedLimitKb = _serverSettings.GetDownloadSpeedLimit(serverMachineIdentifier);

        return new DashMpdCliOptions
        {
            MpdUrl = downloadUrl,
            Output = Path.Combine(downloadTask.DownloadDirectory, downloadTask.FileName),
            WorkingDirectory = downloadTask.DownloadDirectory,
            Quiet = false,
            Quality = "best",
            LimitRate = speedLimitKb > 0 ? $"{speedLimitKb}K" : null,
            EnvironmentVariables = new Dictionary<string, string>
            {
                ["TMPDIR"] = downloadTask.DownloadDirectory,
                ["TMP"] = downloadTask.DownloadDirectory,
            },
        };
    }

    private Task SetupDownloadListeners(DownloadTaskKey key)
    {
        _subscriptions.Add(
            _dashWrapper
                .Progress.Sample(TimeSpan.FromMilliseconds(500))
                .TakeUntil(_destroy)
                .Select(progress =>
                    Observable.FromAsync(async _ =>
                    {
                        await HandleProgressChanged(key, progress);
                    })
                )
                .Concat()
                .Subscribe()
        );

        _subscriptions.Add(
            _dashWrapper
                .StandardOutput.TakeUntil(_destroy)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Subscribe(line => _log.Here().Debug("{Data}", line.Trim()))
        );

        return Task.CompletedTask;
    }

    private async Task HandleProgressChanged(DownloadTaskKey key, DashDownloadProgress progress)
    {
        using var dbContext = await _dbContextFactory.CreateAsync();

        var dataTotal = progress.TotalBytes;
        if (dataTotal <= 0 && progress.Percent > 0)
            dataTotal = progress.DownloadedBytes * 100 / progress.Percent;

        var progressUpdate = new DownloadTaskProgress
        {
            DataTotal = dataTotal,
            Percentage = Convert.ToDecimal(progress.Percent),
            DataReceived = progress.DownloadedBytes,
            DownloadSpeed = progress.Percent < 100 ? progress.DownloadSpeedInBytes : 0,
        };

        await dbContext.UpdateDownloadProgress(key, progressUpdate, cancellationToken: CancellationToken.None);
        await SendProgressLog(progressUpdate);

        if (progressUpdate.Percentage == 100)
            await SetDownloadStatusAsync(DownloadStatus.DownloadFinished);

        await _commandExecutor.Send(new DownloadTaskUpdatedCommand(key), CancellationToken.None);
    }

    private async Task SendProgressLog(DownloadTaskProgress progress)
    {
        var progressMsg = _log.Here()
            .DebugMsg(
                "[DashDownloadTaskProgress {MediaFileName} - {Percentage}% - {Speed}]",
                _filename,
                progress.Percentage.ToString("F2"),
                DataFormat.FormatSpeedString(progress.DownloadSpeed)
            );

        await SendDownloadClientLog(NotificationLevel.Debug, DownloadStatus.Downloading, progressMsg);
    }

    private async Task SetDownloadStatusAsync(DownloadStatus status, Result? errorResult = null)
    {
        if (_downloadTaskKey is null)
            return;

        using var dbContext = await _dbContextFactory.CreateAsync();

        _log.Here()
            .InformationMsg(
                "DownloadTask {DownloadTaskId} ({MediaFileName}) transitioning to {NewStatus}",
                _downloadTaskKey!.Id,
                _filename,
                status
            );

        await dbContext.SetDownloadStatus(_downloadTaskKey, status);
        await _commandExecutor.Send(new DownloadTaskUpdatedCommand(_downloadTaskKey), CancellationToken.None);

        await SendDownloadClientLog(
            status.ToNotificationLevel(),
            status,
            $"Download {_filename} transitioned to status: {status}"
        );

        if (errorResult is not null)
            await SendDownloadClientLog(NotificationLevel.Error, status, errorResult.ToString());
    }

    private async Task SendDownloadClientLog(NotificationLevel logLevel, DownloadStatus status, string message)
    {
        if (_downloadTaskKey is null)
            return;

        using var dbContext = await _dbContextFactory.CreateAsync();
        await dbContext.CreateDownloadClientLog(_downloadTaskKey, logLevel, status, message);
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        _disposed = true;
        _destroy.OnNext(Unit.Default);
        _destroy.OnCompleted();
        _subscriptions.Dispose();
        _destroy.Dispose();

        await _dashWrapper.StopAsync();
        await _dashWrapper.DisposeAsync();
    }
}
