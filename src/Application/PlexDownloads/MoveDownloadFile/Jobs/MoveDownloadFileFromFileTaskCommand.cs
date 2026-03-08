using System.Diagnostics;
using System.IO.Abstractions;
using System.Reactive.Subjects;
using System.Threading.Channels;
using FastEndpoints;
using FluentValidation;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;
using Reaparr.Settings.Contracts;

namespace Reaparr.Application;

public record MoveDownloadFileFromFileTaskCommand(
    DownloadTaskKey Key,
    Subject<IDownloadFileTransferProgress>? MoveDownloadFileProgress = null
) : ICommand<Result>;

public class MoveDownloadFileFromFileTaskCommandValidator : AbstractValidator<MoveDownloadFileFromFileTaskCommand>
{
    public MoveDownloadFileFromFileTaskCommandValidator()
    {
        RuleFor(x => x.Key).NotNull();
        RuleFor(x => x.Key.Id).NotEqual(Guid.Empty);
    }
}

public class MoveDownloadFileFromFileTaskCommandHandler : ICommandHandler<MoveDownloadFileFromFileTaskCommand, Result>
{
    private readonly ILogger _log;
    private readonly ICommandExecutor _commandExecutor;
    private readonly IEventPublisher _eventPublisher;
    private readonly IDownloadTaskUpdateDispatcher _downloadTaskUpdateDispatcher;
    private readonly IReaparrDbContext _dbContext;

    /// <summary>
    /// A separate DbContext instance for progress updates to avoid concurrency issues.
    /// </summary>
    private readonly IReaparrDbContext _dbContextProgress;
    private readonly IFile _file;
    private readonly IDirectory _directory;
    private readonly IPath _path;
    private readonly IDownloadManagerSettings _downloadManagerSettings;

    private string _filename = string.Empty;

    private readonly Channel<IDownloadFileTransferProgress> _progressChannel =
        Channel.CreateBounded<IDownloadFileTransferProgress>(
            new BoundedChannelOptions(1)
            {
                SingleReader = true,
                SingleWriter = true,
                FullMode = BoundedChannelFullMode.DropOldest,
            }
        );

    public MoveDownloadFileFromFileTaskCommandHandler(
        ILogger log,
        ICommandExecutor commandExecutor,
        IEventPublisher eventPublisher,
        IDownloadTaskUpdateDispatcher downloadTaskUpdateDispatcher,
        IReaparrDbContext dbContext,
        IReaparrDbContextFactory dbContextFactory,
        IFile file,
        IDirectory directory,
        IPath path,
        IDownloadManagerSettings downloadManagerSettings
    )
    {
        _log = log.ForContext<MoveDownloadFileFromFileTaskCommandHandler>();
        _commandExecutor = commandExecutor;
        _eventPublisher = eventPublisher;
        _downloadTaskUpdateDispatcher = downloadTaskUpdateDispatcher;
        _dbContext = dbContext;
        _dbContextProgress = dbContextFactory.Create();
        _file = file;
        _directory = directory;
        _path = path;
        _downloadManagerSettings = downloadManagerSettings;
    }

    public async Task<Result> ExecuteAsync(
        MoveDownloadFileFromFileTaskCommand command,
        CancellationToken cancellationToken
    )
    {
        var key = command.Key;
        var moveDownloadFileProgress = command.MoveDownloadFileProgress;

        var downloadTask = await _dbContext.GetDownloadTaskFileAsync(command.Key, CancellationToken.None);
        if (downloadTask == null)
            return ResultExtensions.EntityNotFound(nameof(DownloadTaskGeneric), command.Key.Id).LogError();

        // Resolve paths
        var downloadFilePath = downloadTask.DownloadFilePath;
        var destinationPath = downloadTask.DestinationFilePath;
        _filename = _path.GetFileName(downloadTask.DownloadFilePath);

        _log.Here().Debug("Starting file move process for {DownloadFilePath}", downloadFilePath);

        if (string.IsNullOrWhiteSpace(downloadFilePath) || !_file.Exists(downloadFilePath))
        {
            _log.Here()
                .Debug(
                    "Source file not found at expected path for {DownloadTaskId}, checking fallback locations",
                    key.Id
                );

            string? movedInDownloadsPath = null;
            if (!string.IsNullOrWhiteSpace(downloadFilePath))
                movedInDownloadsPath = downloadFilePath.RemoveReapTempSuffix();

            var destinationExists = !string.IsNullOrWhiteSpace(destinationPath) && _file.Exists(destinationPath);
            var movedInDownloadsExists =
                !string.IsNullOrWhiteSpace(movedInDownloadsPath) && _file.Exists(movedInDownloadsPath);

            if (destinationExists)
            {
                _log.Here()
                    .Warning(
                        "Source file was missing for {DownloadTaskId}, but a completed file already exists at destination. Treating move as finished.",
                        key.Id
                    );

                downloadTask.CurrentFileTransferBytesOffset = downloadTask.DataTotal;
                downloadTask.FileDataTransferred = downloadTask.DataTotal;
                await _dbContext.UpdateDownloadFileTransferProgress(
                    key,
                    downloadTask.ToFileTransferProgress(),
                    cancellationToken
                );
                moveDownloadFileProgress?.OnNext(downloadTask.ToFileTransferProgress());

                await UpdateDownloadTaskStatus(key, DownloadStatus.MoveFinished);
                _log.Here().Debug("Move marked finished via existing destination file for {DownloadTaskId}", key.Id);
                return Result.Ok();
            }

            if (movedInDownloadsExists)
            {
                if (ShouldKeepInDownloads(downloadTask))
                {
                    _log.Here()
                        .Warning(
                            "Source file was missing for {DownloadTaskId}, but renamed file exists in downloads and keep-in-downloads is set. Treating move as finished.",
                            key.Id
                        );

                    downloadTask.CurrentFileTransferBytesOffset = downloadTask.DataTotal;
                    downloadTask.FileDataTransferred = downloadTask.DataTotal;
                    await _dbContext.UpdateDownloadFileTransferProgress(
                        key,
                        downloadTask.ToFileTransferProgress(),
                        cancellationToken
                    );
                    moveDownloadFileProgress?.OnNext(downloadTask.ToFileTransferProgress());

                    await UpdateDownloadTaskStatus(key, DownloadStatus.MoveFinished);
                    _log.Here()
                        .Debug(
                            "Move marked finished via renamed downloads file (keep-in-downloads) for {DownloadTaskId}",
                            key.Id
                        );
                    return Result.Ok();
                }

                // The .reaptemp rename already happened but the file was not moved to the destination yet.
                // Resume the move using the renamed file as the source.
                _log.Here()
                    .Warning(
                        "Source .reaptemp file was missing for {DownloadTaskId} but renamed file exists in downloads. Resuming move to destination.",
                        key.Id
                    );
                downloadFilePath = movedInDownloadsPath!;
            }
            else
            {
                var result = Result
                    .Fail($"Source file does not exist and cannot be moved: {downloadFilePath}")
                    .LogError();
                return await ErrorDownloadTask(key, result);
            }
        }

        try
        {
            if (downloadFilePath.RemoveReapTempSuffix() == destinationPath)
            {
                // Just rename it to remove .reapTemp suffix if present
                _log.Here()
                    .Debug(
                        "Source and destination resolve to the same path for {DownloadTaskId} — renaming in-place to strip .reaptemp suffix",
                        key.Id
                    );

                if (_file.Exists(destinationPath))
                {
                    _log.Here()
                        .Debug("Deleting pre-existing destination file before rename for {DownloadTaskId}", key.Id);
                    Result.Try(() => _file.Delete(destinationPath)).LogIfFailed();
                }

                _file.Move(downloadFilePath.RemoveReapTempSuffix(), destinationPath);

                downloadTask.CurrentFileTransferBytesOffset = downloadTask.DataTotal;
                downloadTask.FileDataTransferred = downloadTask.DataTotal;
                await _dbContext.UpdateDownloadFileTransferProgress(
                    key,
                    downloadTask.ToFileTransferProgress(),
                    cancellationToken
                );
                moveDownloadFileProgress?.OnNext(downloadTask.ToFileTransferProgress());

                await UpdateDownloadTaskStatus(key, DownloadStatus.MoveFinished);
                _log.Here().Debug("In-place rename succeeded for {DownloadTaskId}", key.Id);
                return Result.Ok();
            }

            // Determine if we should keep the file in the downloads directory
            var destinationDirectoryPath = _path.GetDirectoryName(destinationPath);
            var keepInDownloads = ShouldKeepInDownloads(downloadTask);

            _log.Here()
                .Debug(
                    "Move strategy for {DownloadTaskId}: KeepInDownloads={KeepInDownloads}, Source={SourcePath}, Destination={DestinationPath}",
                    key.Id,
                    keepInDownloads,
                    downloadFilePath,
                    destinationPath
                );

            if (keepInDownloads)
            {
                // Rename it to remove .reapTemp suffix if present
                var targetInDownloads = downloadFilePath.RemoveReapTempSuffix();
                _log.Here()
                    .Debug(
                        "Renaming to remove .reaptemp suffix in downloads folder for {DownloadTaskId}: {TargetPath}",
                        key.Id,
                        targetInDownloads
                    );

                if (_file.Exists(targetInDownloads))
                {
                    _log.Here()
                        .Debug("Deleting pre-existing target in downloads before rename for {DownloadTaskId}", key.Id);
                    Result.Try(() => _file.Delete(targetInDownloads)).LogIfFailed();
                }

                _file.Move(downloadFilePath, targetInDownloads);

                downloadTask.CurrentFileTransferBytesOffset = downloadTask.DataTotal;
                downloadTask.FileDataTransferred = downloadTask.DataTotal;
                await _dbContext.UpdateDownloadFileTransferProgress(
                    key,
                    downloadTask.ToFileTransferProgress(),
                    cancellationToken
                );
                moveDownloadFileProgress?.OnNext(downloadTask.ToFileTransferProgress());

                await UpdateDownloadTaskStatus(key, DownloadStatus.MoveFinished);
                _log.Here()
                    .Debug(
                        "Keep-in-downloads rename succeeded for {DownloadTaskId}: {TargetPath}",
                        key.Id,
                        targetInDownloads
                    );
                return Result.Ok();
            }

            // Ensure destination directory exists
            var directoryPathResult = Result.Try(() => _path.GetDirectoryName(destinationPath));
            if (directoryPathResult.IsFailed)
                return await ErrorDownloadTask(key, directoryPathResult.ToResult());

            if (string.IsNullOrEmpty(directoryPathResult.Value))
                return await ErrorDownloadTask(
                    key,
                    Result.Fail($"Could not determine the directory name of path: {directoryPathResult.Value}")
                );

            // Ensure the destination directory exists only when we are actually moving
            _log.Here()
                .Debug(
                    "Ensuring destination directory exists for {DownloadTaskId}: {DestinationDirectory}",
                    key.Id,
                    destinationDirectoryPath
                );
            var createDirectoryResult = Result
                .Try((() => _directory.CreateDirectory(destinationDirectoryPath!)))
                .ToResult();
            if (createDirectoryResult.IsFailed)
                return await ErrorDownloadTask(key, createDirectoryResult);

            // If destination exists, delete to ensure overwrite semantics and reset resume offset
            var destinationAlreadyExists = _file.Exists(destinationPath);
            if (destinationAlreadyExists)
            {
                _log.Here()
                    .Debug(
                        "Destination file already exists for {DownloadTaskId}, deleting to start fresh: {DestinationPath}",
                        key.Id,
                        destinationPath
                    );
                var deleteExistingResult = Result.Try(() => _file.Delete(destinationPath));
                if (deleteExistingResult.IsFailed)
                    return await ErrorDownloadTask(key, deleteExistingResult);

                // Reset offset to start fresh since we removed existing target
                downloadTask.CurrentFileTransferBytesOffset = 0;
                downloadTask.FileDataTransferred = 0;
            }

            // Update status before moving
            await UpdateDownloadTaskStatus(key, DownloadStatus.Moving);

            _log.Here()
                .Debug(
                    "Starting move for {DownloadTaskId}: {SourcePath} -> {DestinationPath} (offset: {Offset} bytes)",
                    key.Id,
                    downloadFilePath,
                    destinationPath,
                    downloadTask.CurrentFileTransferBytesOffset
                );

            var moveResult = await MoveWithResumeAsync(
                downloadTask,
                downloadFilePath,
                destinationPath,
                key,
                cancellationToken
            );
            if (moveResult.IsFailed)
                return await ErrorDownloadTask(key, moveResult);

            _log.Here().Debug("Resumable move completed successfully for {DownloadTaskId}", key.Id);

            // Instant finish on rename
            downloadTask.CurrentFileTransferBytesOffset = downloadTask.DataTotal;
            downloadTask.FileDataTransferred = downloadTask.DataTotal;
            await _dbContext.UpdateDownloadFileTransferProgress(
                key,
                downloadTask.ToFileTransferProgress(),
                cancellationToken
            );
            moveDownloadFileProgress?.OnNext(downloadTask.ToFileTransferProgress());

            await UpdateDownloadTaskStatus(key, DownloadStatus.MoveFinished);
            _log.Here().Debug("Move finished for {DownloadTaskId}: {DestinationPath}", key.Id, destinationPath);
        }
        catch (OperationCanceledException)
        {
            _log.Here().Warning("The file move operation was cancelled for file task {FileTaskId}", key.Id);

            await UpdateDownloadTaskStatus(key, DownloadStatus.MovePaused);
            await _dbContext.UpdateDownloadFileTransferProgress(
                key,
                downloadTask.ToFileTransferProgress(),
                cancellationToken
            );

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _log.Here().Error(ex, "An error occurred while moving file for file task {FileTaskId}", key.Id);
            return await ErrorDownloadTask(key, Result.Fail(new ExceptionalError(ex)));
        }
        finally
        {
            moveDownloadFileProgress?.OnCompleted();
        }

        return Result.Ok();
    }

    private async Task<Result> MoveWithResumeAsync(
        DownloadTaskFileBase downloadTask,
        string sourcePath,
        string targetPath,
        DownloadTaskKey key,
        CancellationToken cancellationToken
    )
    {
        var consumerTask = Task.Run(
            async () =>
            {
                await foreach (var dto in _progressChannel.Reader.ReadAllAsync(cancellationToken))
                {
                    try
                    {
                        await _dbContextProgress.UpdateDownloadFileTransferProgress(
                            key,
                            dto,
                            cancellationToken: cancellationToken
                        );
                    }
                    catch (Exception ex)
                    {
                        _log.Here().Error(ex, "Error while processing file transfer progress for {FileTaskId}", key.Id);
                    }
                }
            },
            cancellationToken
        );

        var stopwatch = Stopwatch.StartNew();

        void SendProgress(MoveFileTransferProgressDTO progress)
        {
            downloadTask.CurrentFileTransferBytesOffset = progress.Transferred;
            downloadTask.FileDataTransferred = progress.Transferred;
            downloadTask.FileTransferSpeed = progress.FileTransferSpeed;

            if (stopwatch.ElapsedMilliseconds > 1000)
            {
                _log.Here().Debug(downloadTask.ToString());
                var fileTransferProgress = downloadTask.ToFileTransferProgress();
                _progressChannel.Writer.TryWrite(fileTransferProgress);
                stopwatch.Restart();
            }
        }

        try
        {
            var result = await _commandExecutor.Send(
                new MoveFileWithResumeCommand
                {
                    SourcePath = sourcePath,
                    TargetPath = targetPath,
                    Progress = SendProgress,
                    CurrentOffset = downloadTask.CurrentFileTransferBytesOffset,
                    DataTotal = downloadTask.DataTotal,
                },
                cancellationToken
            );

            return result;
        }
        finally
        {
            _progressChannel.Writer.TryComplete();
            await consumerTask;
        }
    }

    private async Task UpdateDownloadTaskStatus(DownloadTaskKey key, DownloadStatus status)
    {
        await _downloadTaskUpdateDispatcher.OnStatusChangedAsync(key, status);
        await _dbContext.CreateDownloadClientLog(
            key,
            NotificationLevel.Information,
            status,
            $"DownloadTask {key.Id} ({_filename}) has transitioned to {status}"
        );
    }

    private async Task<Result> ErrorDownloadTask(DownloadTaskKey key, Result result)
    {
        await _downloadTaskUpdateDispatcher.OnStatusChangedAsync(key, DownloadStatus.MoveError);
        await _dbContext.CreateDownloadClientLog(
            key,
            NotificationLevel.Error,
            DownloadStatus.MoveError,
            result.ToString()
        );

        await _eventPublisher.PublishAsync(new SendNotificationResult(result), CancellationToken.None);

        return result.LogError();
    }

    private bool ShouldKeepInDownloads(DownloadTaskFileBase downloadTask) =>
        downloadTask.DirectoryMeta.KeepCompletedInDownloadFolder
        || _downloadManagerSettings.KeepCompletedInDownloadFolder
        || downloadTask.DownloadFilePath.RemoveReapTempSuffix() == downloadTask.DestinationFilePath
        || string.IsNullOrWhiteSpace(downloadTask.DestinationFilePath);
}
