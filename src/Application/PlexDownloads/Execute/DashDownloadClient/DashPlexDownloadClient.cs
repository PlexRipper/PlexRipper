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
    private readonly IDashMpdCliWrapper _dashWrapper;
    private readonly ICommandExecutor _commandExecutor;
    private readonly IDownloadTaskUpdateDispatcher _downloadTaskUpdateDispatcher;
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
        IDownloadTaskUpdateDispatcher downloadTaskUpdateDispatcher,
        IServerSettingsModule serverSettings,
        IDirectory directory
    )
    {
        _log = log.ForContext<DashPlexDownloadClient>();
        _dashWrapper = dashWrapper;
        _commandExecutor = commandExecutor;
        _downloadTaskUpdateDispatcher = downloadTaskUpdateDispatcher;
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
        {
            var status = downloadUrlResult.ToResult().Has504GatewayTimeoutError()
                ? DownloadStatus.ServerUnreachable
                : DownloadStatus.SourceUnavailable;
            await SetDownloadStatusAsync(status, downloadUrlResult.ToResult());
            return downloadUrlResult.ToResult().LogError();
        }

        // Create working directory
        var createDirectoryResult = Result.Try(() => _directory.CreateDirectory(downloadTask.DownloadDirectory));
        if (createDirectoryResult.IsFailed)
        {
            await SetDownloadStatusAsync(DownloadStatus.StorageError, createDirectoryResult.ToResult());
            return createDirectoryResult.ToResult();
        }

        SetupDownloadListeners(downloadTaskKey);

        // Execute dash stream download
        await SetDownloadStatusAsync(DownloadStatus.Downloading);
        var options = await CreateDashOptions(downloadTask, downloadUrlResult.Value, cancellationToken);
        await using var cancellationRegistration = cancellationToken.Register(() =>
        {
            _ = _dashWrapper.StopAsync();
        });
        var startResult = await _dashWrapper.StartAsync(options);
        if (startResult.IsCancelled || startResult.IsFailed)
            return startResult;

        return Result.Ok();
    }

    public async Task<Result> StopAsync()
    {
        var stopResult = await _dashWrapper.StopAsync();
        if (stopResult.IsFailed)
            return stopResult;

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
            Output = downloadTask.DownloadFilePath,
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

    private void SetupDownloadListeners(DownloadTaskKey key)
    {
        _subscriptions.Add(
            _dashWrapper
                .Progress.Sample(TimeSpan.FromMilliseconds(300))
                .TakeUntil(_destroy)
                .Subscribe(progress =>
                {
                    var progressUpdate = new DownloadTaskProgress
                    {
                        DataTotal = progress.TotalBytes,
                        Percentage = Convert.ToDecimal(progress.Percent),
                        DataReceived = progress.DownloadedBytes,
                        DownloadSpeed = progress.DownloadSpeedInBytes,
                        TimeRemaining = progress.ETA,
                    };

                    _downloadTaskUpdateDispatcher.OnProgressUpdated(key, progressUpdate);
                })
        );

        _subscriptions.Add(
            _dashWrapper
                .DownloadCompleted.TakeUntil(_destroy)
                .Select(completed =>
                    Observable.FromAsync(async _ =>
                    {
                        await HandleDownloadCompleted(key, completed);
                    })
                )
                .Concat()
                .Subscribe()
        );
    }

    private async Task HandleDownloadCompleted(DownloadTaskKey key, DashDownloadCompletedEventArgs completed)
    {
        if (completed.Cancelled)
        {
            await SetDownloadStatusAsync(DownloadStatus.Stopped, completed.Result);
            return;
        }

        if (!completed.IsSuccess)
        {
            var status = completed.Result.Has504GatewayTimeoutError()
                ? DownloadStatus.ServerUnreachable
                : DownloadStatus.DownloadClientError;
            await SetDownloadStatusAsync(status, completed.Result);
            return;
        }

        await SetDownloadStatusAsync(DownloadStatus.DownloadFinished);
    }

    private async Task SetDownloadStatusAsync(DownloadStatus status, Result? errorResult = null)
    {
        if (_downloadTaskKey is null)
            return;

        if (errorResult is null)
        {
            await _downloadTaskUpdateDispatcher.OnStatusChangedAsync(_downloadTaskKey, status, CancellationToken.None);
            return;
        }

        await _downloadTaskUpdateDispatcher.OnStatusChangedAsync(
            _downloadTaskKey,
            status,
            errorResult,
            CancellationToken.None
        );
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
        _dbContext.Dispose();
    }
}
