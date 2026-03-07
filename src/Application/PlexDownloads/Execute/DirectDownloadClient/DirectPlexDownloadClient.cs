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

    public DirectPlexDownloadClient(
        ILogger log,
        IReaparrDbContextFactory dbContextFactory,
        ICommandExecutor commandExecutor,
        IDownloadManagerSettings downloadManagerSettings,
        IServerSettingsModule serverSettings,
        Func<DownloadConfiguration, IDownloadService> downloadServiceFactory
    )
    {
        _log = log.ForContext<DirectPlexDownloadClient>();
        _dbContextFactory = dbContextFactory;
        _commandExecutor = commandExecutor;
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

        var downloadUrlResult = await _dbContext.GetDownloadUrl(
            downloadTask.PlexServerId,
            downloadTask.FileLocationUrl,
            cancellationToken
        );

        if (downloadUrlResult.IsFailed)
            return downloadUrlResult.ToResult();

        var downloadUrl = downloadUrlResult.Value;

        // Prepare destination stream
        var fileStreamResult = await _commandExecutor.Send(
            new CreateDownloadFileStreamCommand(
                downloadTask.DownloadDirectory,
                downloadTask.FileName,
                downloadTask.DataTotal
            ),
            cancellationToken
        );

        if (fileStreamResult.IsFailed)
        {
            await SetDownloadStatusAsync(Domain.DownloadStatus.StorageError, fileStreamResult.ToResult());
            return fileStreamResult.ToResult();
        }

        await using var fileStream = fileStreamResult.Value;
        _filename = downloadTask.FileName;

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
            await _downloader.DownloadFileTaskAsync(downloadUrl, downloadTask.DownloadFilePath, cancellationToken);
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
                .Sample(TimeSpan.FromMilliseconds(500))
                .Select(args =>
                    Observable.FromAsync(async _ =>
                    {
                        using var dbContext = await _dbContextFactory.CreateAsync();
                        var progress = new DownloadTaskProgress
                        {
                            DataTotal = args.TotalBytesToReceive,
                            Percentage = Convert.ToDecimal(args.ProgressPercentage),
                            DataReceived = args.ReceivedBytesSize,
                            DownloadSpeed =
                                args.ProgressPercentage < 100 ? Convert.ToInt64(args.BytesPerSecondSpeed) : 0,
                        };

                        await dbContext.UpdateDownloadProgress(
                            key,
                            progress,
                            _downloader.Package.ToSnapshot(),
                            CancellationToken.None
                        );

                        await SendProgressLog(progress);

                        await _commandExecutor.Send(new DownloadTaskUpdatedCommand(key), CancellationToken.None);
                    })
                )
                .Concat()
                .Subscribe()
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
                            await SetDownloadStatusAsync(Domain.DownloadStatus.Paused);
                            return;
                        }

                        if (args.Error != null)
                        {
                            await SetDownloadStatusAsync(
                                Domain.DownloadStatus.Error,
                                Result.Fail(new ExceptionalError(args.Error)).LogError()
                            );
                            return;
                        }

                        // Download completed successfully
                        using var dbContext = await _dbContextFactory.CreateAsync();
                        var progress = new DownloadTaskProgress
                        {
                            DataTotal = package!.TotalFileSize,
                            Percentage = 100,
                            DataReceived = package.ReceivedBytesSize,
                            DownloadSpeed = 0,
                        };

                        await dbContext.UpdateDownloadProgress(
                            key,
                            progress,
                            package.ToSnapshot(),
                            CancellationToken.None
                        );

                        await SendProgressLog(progress);

                        await SetDownloadStatusAsync(Domain.DownloadStatus.DownloadFinished);
                        await _commandExecutor.Send(new DownloadTaskUpdatedCommand(key), CancellationToken.None);
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
        var progressMsg = _log.Here(sourceFilePath, memberName, sourceLineNumber)
            .DebugMsg(
                "[DownloadTaskProgress {MediaFileName} - {Percentage}% - {Speed} - {DataReceived} / {DataTotal} - {TimeRemaining}]",
                _filename,
                progress.Percentage.ToString("F2"),
                DataFormat.FormatSpeedString(progress.DownloadSpeed),
                ByteSize.FromBytes(progress.DataReceived).ToString("MB"),
                ByteSize.FromBytes(progress.DataTotal).ToString("MB"),
                TimeSpan
                    .FromSeconds(
                        DataFormat.GetTimeRemaining(progress.DataTotal - progress.DataReceived, progress.DownloadSpeed)
                    )
                    .ToFormattedString()
            );

        await SendDownloadClientLog(NotificationLevel.Debug, Domain.DownloadStatus.Downloading, progressMsg);
    }

    private async Task SetDownloadStatusAsync(Domain.DownloadStatus status, Result? errorResult = null)
    {
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
    }
}
