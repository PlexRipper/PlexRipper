using System.ComponentModel;
using System.IO.Abstractions;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using ByteSizeLib;
using Downloader;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;
using Reaparr.Settings.Contracts;

namespace Reaparr.Application;

public class DirectPlexDownloadClient : IPlexDownloadClient
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly IReaparrDbContextFactory _dbContextFactory;
    private readonly ICommandExecutor _commandExecutor;
    private readonly IDownloadTaskUpdateDispatcher _downloadTaskUpdateDispatcher;
    private readonly IServerSettingsModule _serverSettings;

    private DownloadTaskKey? _downloadTaskKey;
    private string _filename = string.Empty;

    private readonly IDownloadService _downloader;
    private readonly DownloadConfiguration _configuration = new()
    {
        DownloadFileExtension = FilePathExtensions.TempDownloadFileSuffix,
    };

    private readonly CompositeDisposable _subscriptions = new();
    private readonly Subject<Unit> _destroy = new();
    private int _isDisposed;
    private long _lastLoggedDataReceived = -1;
    private decimal _lastLoggedPercentage = -1;

    public DirectPlexDownloadClient(
        ILogger log,
        IReaparrDbContextFactory dbContextFactory,
        ICommandExecutor commandExecutor,
        IDownloadTaskUpdateDispatcher downloadTaskUpdateDispatcher,
        IDownloadManagerSettings downloadManagerSettings,
        IServerSettingsModule serverSettings,
        Func<DownloadConfiguration, IDownloadService> downloadServiceFactory
    )
    {
        _log = log.ForContext<DirectPlexDownloadClient>();
        _dbContextFactory = dbContextFactory;
        _commandExecutor = commandExecutor;
        _downloadTaskUpdateDispatcher = downloadTaskUpdateDispatcher;
        _dbContext = dbContextFactory.Create();
        _serverSettings = serverSettings;

        var downloadSegments = Math.Max(1, downloadManagerSettings.DownloadSegments);

        // Number of file parts, default is 1
        _configuration.ChunkCount = downloadSegments;
        _configuration.ParallelCount = downloadSegments;
        _configuration.ParallelDownload = downloadSegments > 1;

        _downloader = downloadServiceFactory(_configuration);
    }

    /// <inheritdoc/>
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

        var downloadUrlResult = await _commandExecutor.Send(
            new GetDirectDownloadUrlCommand(downloadTask.PlexServerId, downloadTask.FileLocationUrl),
            cancellationToken
        );
        if (downloadUrlResult.IsFailed)
        {
            var failedResult = downloadUrlResult.ToResult();
            var failureStatus = failedResult.IsServerUnreachable()
                ? Domain.DownloadStatus.ServerUnreachable
                : Domain.DownloadStatus.Error;

            await SendDownloadClientLog(NotificationLevel.Error, failureStatus, failedResult.ToString());

            var statusResult = await SetDownloadStatusAsync(failureStatus);
            if (statusResult.IsFailed)
                return statusResult;

            return failedResult;
        }

        var downloadUrl = downloadUrlResult.Value;

        // Ensure the download directory exists and has enough disk space
        var ensureDirectoryResult = await _commandExecutor.Send(
            new EnsureDownloadDirectoryCommand(downloadTask.DownloadDirectory, downloadTask.DataTotal),
            cancellationToken
        );

        if (ensureDirectoryResult.IsFailed)
        {
            var statusResult = await SetDownloadStatusAsync(Domain.DownloadStatus.StorageError, ensureDirectoryResult);
            if (statusResult.IsFailed)
                return statusResult;
            return ensureDirectoryResult;
        }

        await SetupDownloadListeners(downloadTaskKey);

        if (downloadTask.DirectDownloadSnapshot is not null)
        {
            await SendDownloadClientLog(
                NotificationLevel.Information,
                Domain.DownloadStatus.Downloading,
                $"Resuming {_filename} download from pause"
            );
            var progress = downloadTask.DirectDownloadSnapshot.ToDownloadPackage();
            progress.Urls = [downloadUrl]; // Ensure we use the latest connection string
            await _downloader.DownloadFileTaskAsync(progress, cancellationToken);
        }
        else
        {
            var downloaderTargetPath = Path.Combine(downloadTask.DownloadDirectory, downloadTask.FileName);
            await _downloader.DownloadFileTaskAsync(downloadUrl, downloaderTargetPath, cancellationToken);
        }

        return Result.Ok();
    }

    /// <inheritdoc/>
    public async Task<Result> StopAsync()
    {
        if (_downloadTaskKey is null)
            return Result.Ok();

        _log.Here()
            .Debug(
                "DownloadTask {DownloadTaskId} ({MediaFileName}) has been requested to stop.",
                _downloadTaskKey.Id,
                _filename
            );

        await _downloader.CancelTaskAsync();

        var stopMsg = _log.Here()
            .InformationMsg(
                "DownloadTask {DownloadTaskId} ({MediaFileName}) was stopped.",
                _downloadTaskKey.Id,
                _filename
            );

        await SendDownloadClientLog(NotificationLevel.Information, Domain.DownloadStatus.Stopped, stopMsg);
        return Result.Ok();
    }

    private async Task SetupDownloadListeners(DownloadTaskKey key)
    {
        // Setup DownloadLimit Subscription
        var serverMachineIdentifier = await _dbContext.GetPlexServerMachineIdentifierById(key.PlexServerId);
        _subscriptions.Add(
            _serverSettings
                .GetDownloadSpeedLimitObservable(serverMachineIdentifier)
                .TakeUntil(_destroy)
                .Subscribe(value =>
                {
                    _configuration.MaximumBytesPerSecond = Math.Max(0, value) * 1024;
                })
        );

        // Setup DownloadStarted Subscription
        _subscriptions.Add(
            Observable
                .FromEventPattern<DownloadStartedEventArgs>(
                    h => _downloader.DownloadStarted += h,
                    h => _downloader.DownloadStarted -= h
                )
                .Select(x => x.EventArgs)
                .Take(1)
                .Select(_ =>
                    Observable.FromAsync(async _ =>
                    {
                        await SetDownloadStatusAsync(Domain.DownloadStatus.Downloading);
                    })
                )
                .Concat()
                .Subscribe()
        );

        // Setup DownloadProgressChanged Subscription
        _subscriptions.Add(
            Observable
                .FromEventPattern<DownloadProgressChangedEventArgs>(
                    h => _downloader.DownloadProgressChanged += h,
                    h => _downloader.DownloadProgressChanged -= h
                )
                .Select(x => x.EventArgs)
                .Sample(TimeSpan.FromMilliseconds(300))
                .Subscribe(args =>
                {
                    var progress = new DownloadTaskProgress
                    {
                        DataTotal = args.TotalBytesToReceive,
                        Percentage = Convert.ToDecimal(args.ProgressPercentage),
                        DataReceived = args.ReceivedBytesSize,
                        DownloadSpeed = args.ProgressPercentage < 100 ? Convert.ToInt64(args.BytesPerSecondSpeed) : 0,
                        TimeRemaining = DataFormat.GetTimeRemaining(
                            args.TotalBytesToReceive - args.ReceivedBytesSize,
                            args.BytesPerSecondSpeed
                        ),
                    };

                    _downloadTaskUpdateDispatcher.OnProgressUpdated(key, progress, _downloader.Package.ToSnapshot());
                    _ = SendProgressLog(progress);
                })
        );

        _subscriptions.Add(
            Observable
                .FromEventPattern<AsyncCompletedEventArgs>(
                    h => _downloader.DownloadFileCompleted += h,
                    h => _downloader.DownloadFileCompleted -= h
                )
                .Select(x => x.EventArgs)
                .Take(1)
                .Select(args =>
                    Observable.FromAsync(async _ =>
                    {
                        var package = args.UserState as DownloadPackage;
                        _log.Here().Debug("The UserState at time of completion: {@Package}", package);
                        if (args.Cancelled)
                        {
                            var statusResult = await SetDownloadStatusAsync(Domain.DownloadStatus.Paused);
                            statusResult.LogIfFailed();
                            return;
                        }

                        if (args.Error != null)
                        {
                            var statusResult = await SetDownloadStatusAsync(
                                Domain.DownloadStatus.Error,
                                Result.Fail(new ExceptionalError(args.Error)).LogError()
                            );
                            statusResult.LogIfFailed();
                            return;
                        }

                        // Download completed successfully
                        var progress = new DownloadTaskProgress
                        {
                            DataTotal = package!.TotalFileSize,
                            Percentage = 100,
                            DataReceived = Math.Max(package.ReceivedBytesSize, package.TotalFileSize),
                            DownloadSpeed = 0,
                            TimeRemaining = 0,
                        };

                        _downloadTaskUpdateDispatcher.OnProgressUpdated(key, progress, package.ToSnapshot());

                        await SendProgressLog(progress);

                        var finishResult = await SetDownloadStatusAsync(Domain.DownloadStatus.DownloadFinished);
                        finishResult.LogIfFailed();
                    })
                )
                .Concat()
                .Subscribe()
        );
    }

    private async Task SendProgressLog(
        DownloadTaskProgress progress,
        [CallerFilePath] string sourceFilePath = "",
        [CallerMemberName] string memberName = "",
        [CallerLineNumber] int sourceLineNumber = 0
    )
    {
        if (_lastLoggedDataReceived == progress.DataReceived && _lastLoggedPercentage == progress.Percentage)
            return;

        _lastLoggedDataReceived = progress.DataReceived;
        _lastLoggedPercentage = progress.Percentage;

        var progressMsg = _log.Here(sourceFilePath, memberName, sourceLineNumber)
            .DebugMsg(
                "[DownloadTaskProgress {MediaFileName} - {Percentage}% - {Speed} - {DataReceived} / {DataTotal} - {TimeRemaining}]",
                _filename,
                progress.Percentage.ToString("F2"),
                DataFormat.FormatSpeedString(progress.DownloadSpeed),
                ByteSize.FromBytes(progress.DataReceived).ToString("MB"),
                ByteSize.FromBytes(progress.DataTotal).ToString("MB"),
                TimeSpan.FromSeconds(progress.TimeRemaining).ToFormattedString()
            );

        await SendDownloadClientLog(NotificationLevel.Debug, Domain.DownloadStatus.Downloading, progressMsg);
    }

    private async Task<Result> SetDownloadStatusAsync(Domain.DownloadStatus status, Result? errorResult = null)
    {
        if (_downloadTaskKey is null)
            return Result.Ok();

        if (errorResult is null)
        {
            var updateResult = await _downloadTaskUpdateDispatcher.OnStatusChangedAsync(
                _downloadTaskKey,
                status,
                CancellationToken.None
            );
            if (updateResult.IsFailed)
                return updateResult;

            return Result.Ok();
        }

        var erroredUpdateResult = await _downloadTaskUpdateDispatcher.OnStatusChangedAsync(
            _downloadTaskKey,
            status,
            errorResult,
            CancellationToken.None
        );
        if (erroredUpdateResult.IsFailed)
            return erroredUpdateResult;

        return Result.Ok();
    }

    private async Task SendDownloadClientLog(NotificationLevel logLevel, Domain.DownloadStatus status, string message)
    {
        using var dbContext = await _dbContextFactory.CreateAsync();
        await dbContext.CreateDownloadClientLog(_downloadTaskKey!, logLevel, status, message);
    }

    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _isDisposed, 1) == 1)
            return;

        // signals completion to all streams
        _destroy.OnNext(Unit.Default);
        _destroy.OnCompleted();
        _subscriptions.Dispose();
        _destroy.Dispose();

        if (_downloader is IAsyncDisposable asyncDisposableDownloader)
        {
            await asyncDisposableDownloader.DisposeAsync();
        }
        else if (_downloader is IDisposable disposableDownloader)
        {
            disposableDownloader.Dispose();
        }

        _dbContext.Dispose();
    }
}
