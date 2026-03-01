using System.ComponentModel;
using System.IO.Abstractions;
using System.Reactive.Linq;
using Downloader;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;
using Reaparr.Settings.Contracts;

namespace Reaparr.Application;

public class PlexDownloadClient : IPlexDownloadClient
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly IReaparrDbContextFactory _dbContextFactory;
    private readonly ICommandExecutor _commandExecutor;
    private readonly IServerSettingsModule _serverSettings;
    private readonly IPath _path;

    private DownloadTaskKey? _downloadTaskKey;
    private string _filename = string.Empty;

    private IDownloadService _downloader = new DownloadService();
    private readonly DownloadConfiguration _configuration = new()
    {
        DownloadFileExtension = FilePathExtensions.TEMP_DOWNLOAD_FILE_SUFFIX,
    };

    public PlexDownloadClient(
        ILogger log,
        IReaparrDbContextFactory dbContextFactory,
        ICommandExecutor commandExecutor,
        IDownloadManagerSettings downloadManagerSettings,
        IServerSettingsModule serverSettings,
        IPath path
    )
    {
        _log = log.ForContext<PlexDownloadClient>();
        _dbContextFactory = dbContextFactory;
        _commandExecutor = commandExecutor;
        _dbContext = dbContextFactory.Create();
        _serverSettings = serverSettings;
        _path = path;

        var downloadSegments = downloadManagerSettings.DownloadSegments;

        // Number of file parts, default is 1
        _configuration.ChunkCount = downloadSegments;
        _configuration.ParallelCount = downloadSegments;
        _configuration.ParallelDownload = downloadSegments > 1;
    }

    /// <inheritdoc/>
    public async Task<Result> Start(DownloadTaskKey downloadTaskKey, CancellationToken cancellationToken = default)
    {
        _downloadTaskKey = downloadTaskKey;
        var downloadTask = await _dbContext.GetDownloadTaskAsync(downloadTaskKey, cancellationToken);
        if (downloadTask is null)
        {
            return ResultExtensions
                .EntityNotFound(nameof(DownloadTaskGeneric), downloadTaskKey.ToString())
                .LogWarning();
        }

        var downloadUrlResult = await _dbContext.GetDownloadUrl(
            downloadTask.PlexServerId,
            downloadTask.FileLocationUrl,
            CancellationToken.None
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
            CancellationToken.None
        );

        if (fileStreamResult.IsFailed)
            return fileStreamResult.ToResult();

        var fileStream = fileStreamResult.Value;
        _filename = downloadTask.FileName;
        var filePath = _path.Combine(downloadTask.DownloadDirectory, downloadTask.FileName);

        _downloader = new DownloadService(_configuration);

        await SetupDownloadListeners(downloadTaskKey);

        await _downloader.DownloadFileTaskAsync(downloadUrl, filePath, cancellationToken);

        return Result.Ok();
    }

    /// <inheritdoc/>
    public async Task<Result> StopAsync()
    {
        await _downloader.CancelTaskAsync();

        return Result.Ok();
    }

    private async Task SetupDownloadListeners(DownloadTaskKey key)
    {
        // Setup DownloadLimit Subscription
        var serverMachineIdentifier = await _dbContext.GetPlexServerMachineIdentifierById(key.PlexServerId);
        _serverSettings
            .GetDownloadSpeedLimitObservable(serverMachineIdentifier)
            .Subscribe(value =>
            {
                _configuration.MaximumBytesPerSecond = Math.Max(0, value) * 1024;
            });

        // Setup DownloadStarted Subscription
        Observable
            .FromEventPattern<DownloadStartedEventArgs>(
                h => _downloader.DownloadStarted += h,
                h => _downloader.DownloadStarted -= h
            )
            .Select(x => x.EventArgs)
            .Select(args =>
                Observable.FromAsync(async _ =>
                {
                    await SetDownloadStatusAsync(Domain.DownloadStatus.Downloading);
                })
            )
            .Concat()
            .Subscribe();

        // Setup DownloadProgressChanged Subscription
        Observable
            .FromEventPattern<DownloadProgressChangedEventArgs>(
                h => _downloader.DownloadProgressChanged += h,
                h => _downloader.DownloadProgressChanged -= h
            )
            .Select(x => x.EventArgs)
            .Sample(TimeSpan.FromMilliseconds(500))
            .Select(args =>
                Observable.FromAsync(async ct =>
                {
                    using var dbContext = _dbContextFactory.CreateAsync();
                    var progress = new DownloadTaskProgress
                    {
                        DataTotal = args.TotalBytesToReceive,
                        Percentage = Convert.ToDecimal(args.ProgressPercentage),
                        DataReceived = args.ReceivedBytesSize,
                        DownloadSpeed = Convert.ToInt64(args.AverageBytesPerSecondSpeed),
                    };

                    await _dbContext.UpdateDownloadProgress(key, progress, cancellationToken: ct);

                    _log.Here()
                        .Debug(
                            "[DownloadTaskProgress {MediaFileName} - {Percentage}% - {Speed} - {DataReceived} / {DataTotal} - {TimeRemaining}]",
                            _filename,
                            progress.Percentage.ToString("F2"),
                            DataFormat.FormatSpeedString(progress.DownloadSpeed),
                            progress.DataReceived,
                            progress.DataTotal,
                            DataFormat.GetTimeRemaining(
                                progress.DataTotal - progress.DataReceived,
                                progress.DownloadSpeed
                            )
                        );

                    await _commandExecutor.Send(new DownloadTaskUpdatedCommand(key), CancellationToken.None);
                })
            )
            .Concat()
            .Subscribe();

        Observable
            .FromEventPattern<AsyncCompletedEventArgs>(
                h => _downloader.DownloadFileCompleted += h,
                h => _downloader.DownloadFileCompleted -= h
            )
            .Select(x => x.EventArgs)
            .Select(args =>
                Observable.FromAsync(async ct =>
                {
                    using var dbContext = _dbContextFactory.CreateAsync();
                    if (!args.Cancelled)
                    {
                        await SetDownloadStatusAsync(Domain.DownloadStatus.DownloadFinished);
                    }
                })
            )
            .Concat()
            .Subscribe();
    }

    private async Task SetDownloadStatusAsync(Domain.DownloadStatus status, Result? errorResult = null)
    {
        using var dbContext = await _dbContextFactory.CreateAsync();

        _log.Here()
            .Information(
                "DownloadTask {DownloadTaskId} ({MediaFileName}) transitioning  to {NewStatus}",
                _downloadTaskKey!.Id,
                _filename,
                status
            );

        await dbContext.SetDownloadStatus(_downloadTaskKey, status);
        await _commandExecutor.Send(new DownloadTaskUpdatedCommand(_downloadTaskKey), CancellationToken.None);

        await SendDownloadClientLog(status.ToNotificationLevel(), status, $"Download {status}: {_filename}");

        if (errorResult is not null)
            await SendDownloadClientLog(NotificationLevel.Error, status, errorResult.ToString());
    }

    private async Task SendDownloadClientLog(NotificationLevel logLevel, Domain.DownloadStatus status, string message)
    {
        using var dbContext = await _dbContextFactory.CreateAsync();

        await dbContext.DownloadWorkerTasksLogs.AddAsync(
            new DownloadWorkerLog
            {
                Message = message,
                LogLevel = logLevel,
                CreatedAt = DateTime.UtcNow,
                Status = status,
                DownloadTaskId = _downloadTaskKey!.Id,
            }
        );
        await dbContext.SaveChangesAsync(CancellationToken.None);
    }

    public async ValueTask DisposeAsync() { }
}
