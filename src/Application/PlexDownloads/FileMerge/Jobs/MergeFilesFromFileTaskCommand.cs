using System.Diagnostics;
using System.IO.Abstractions;
using System.Reactive.Subjects;
using FastEndpoints;
using FluentValidation;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;
using Reaparr.Settings.Contracts;

namespace Reaparr.Application;

public record MergeFilesFromFileTaskCommand(
    DownloadTaskKey Key,
    Subject<IDownloadFileTransferProgress>? FileMergeProgress = null
) : ICommand<Result>;

public class MergeFilesFromFileTaskCommandValidator : AbstractValidator<MergeFilesFromFileTaskCommand>
{
    public MergeFilesFromFileTaskCommandValidator()
    {
        RuleFor(x => x.Key).NotNull();
        RuleFor(x => x.Key.Id).NotEqual(Guid.Empty);
    }
}

public class MergeFilesFromFileTaskCommandHandler : ICommandHandler<MergeFilesFromFileTaskCommand, Result>
{
    private readonly Serilog.ILogger _log;
    private readonly ICommandExecutor _commandExecutor;
    private readonly IEventPublisher _eventPublisher;
    private readonly IReaparrDbContext _dbContext;
    private readonly IFile _file;
    private readonly IDirectory _directory;
    private readonly IPath _path;
    private readonly IDownloadManagerSettings _downloadManagerSettings;

    private Stream? _readStream;
    private Stream? _writeStream;

    /// <summary>
    /// Based on https://github.com/dotnet/runtime/discussions/74405#discussioncomment-3488674
    /// 1048576 bytes = 1 MB
    /// </summary>
    private readonly int _bufferSize = 1048576;

    public MergeFilesFromFileTaskCommandHandler(
        ILogger log,
        ICommandExecutor commandExecutor,
        IEventPublisher eventPublisher,
        IReaparrDbContext dbContext,
        IFile file,
        IDirectory directory,
        IPath path,
        IDownloadManagerSettings downloadManagerSettings
    )
    {
        _log = log.ForContext<MergeFilesFromFileTaskCommandHandler>();
        _commandExecutor = commandExecutor;
        _eventPublisher = eventPublisher;
        _dbContext = dbContext;
        _file = file;
        _directory = directory;
        _path = path;
        _downloadManagerSettings = downloadManagerSettings;
    }

    public async Task<Result> ExecuteAsync(MergeFilesFromFileTaskCommand command, CancellationToken cancellationToken)
    {
        var key = command.Key;
        var fileMergeProgress = command.FileMergeProgress;

        var downloadTask = await _dbContext.GetDownloadTaskFileAsync(command.Key, CancellationToken.None);
        if (downloadTask == null)
        {
            return ResultExtensions.EntityNotFound(nameof(DownloadTaskGeneric), command.Key.Id).LogError();
        }

        // Resolve paths
        var tempOrSourcePath = downloadTask.FilePath;
        var finalPathInDownloads = _path.Combine(downloadTask.DownloadDirectory, downloadTask.FileName);
        var destinationPath = downloadTask.DestinationFilePath;

        _log.Here().Debug("Starting file move process for {FileName}", downloadTask.FileName);

        try
        {
            // Ensure destination directory exists
            var directoryPathResult = Result.Try(() => _path.GetDirectoryName(destinationPath));
            if (directoryPathResult.IsFailed)
                return directoryPathResult.ToResult();

            if (string.IsNullOrEmpty(directoryPathResult.Value))
                return Result.Fail($"Could not determine the directory name of path: {directoryPathResult.Value}");

            var createDirectoryResult = Result
                .Try((() => _directory.CreateDirectory(directoryPathResult.Value)))
                .ToResult();
            if (createDirectoryResult.IsFailed)
                return await ErrorDownloadTask(key, createDirectoryResult);

            // If a final-named file already exists in downloads, prefer it and remove temp
            if (_file.Exists(finalPathInDownloads))
            {
                if (
                    _file.Exists(tempOrSourcePath)
                    && !string.Equals(tempOrSourcePath, finalPathInDownloads, StringComparison.OrdinalIgnoreCase)
                )
                    Result.Try((() => _file.Delete(tempOrSourcePath))).LogIfFailed();

                tempOrSourcePath = finalPathInDownloads;
            }

            if (string.IsNullOrWhiteSpace(tempOrSourcePath) || !_file.Exists(tempOrSourcePath))
            {
                var result = Result
                    .Fail($"Source file does not exist and cannot be moved: {tempOrSourcePath}")
                    .LogError();
                return await ErrorDownloadTask(key, result);
            }

            // Update status
            await UpdateDownloadTaskStatus(key, DownloadStatus.Moving);

            // Toggle decision (per task overrides global when true)
            var keepInDownloads =
                downloadTask.DirectoryMeta.KeepCompletedInDownloadFolder
                || _downloadManagerSettings.KeepCompletedInDownloadFolder;

            var targetPath = keepInDownloads ? finalPathInDownloads : destinationPath;

            // If target already exists, delete to replace
            Result
                .Try(
                    (
                        () =>
                        {
                            if (_file.Exists(targetPath))
                                _file.Delete(targetPath);
                        }
                    )
                )
                .LogIfFailed();

            // If source and target are the same, finish immediately
            if (string.Equals(tempOrSourcePath, targetPath, StringComparison.OrdinalIgnoreCase))
            {
                downloadTask.CurrentFileTransferBytesOffset = downloadTask.DataTotal;
                downloadTask.FileDataTransferred = downloadTask.DataTotal;
                await _dbContext.UpdateDownloadFileTransferProgress(key, downloadTask.ToFileTransferProgress());
                fileMergeProgress?.OnNext(downloadTask.ToFileTransferProgress());

                await UpdateDownloadTaskStatus(key, DownloadStatus.MoveFinished);
                return Result.Ok();
            }

            // Try fast rename move first
            var moveResult = Result.Try((() => _file.Move(tempOrSourcePath, targetPath)));
            if (moveResult.IsFailed)
            {
                var copyResult = await CopyWithResumeAsync(
                    downloadTask,
                    tempOrSourcePath,
                    targetPath,
                    fileMergeProgress,
                    key,
                    cancellationToken
                );
                if (copyResult.IsFailed)
                    return await ErrorDownloadTask(key, copyResult);

                // After a successful copy, remove the original temp if still present and different from target
                Result.Try((() =>
                            {
                                if (!string.Equals(tempOrSourcePath, targetPath, StringComparison.OrdinalIgnoreCase)
                                    && _file.Exists(tempOrSourcePath))
                                    _file.Delete(tempOrSourcePath);
                            }
                        )
                    )
                    .LogIfFailed();
            }
            else
            {
                // Instant finish on rename
                downloadTask.CurrentFileTransferBytesOffset = downloadTask.DataTotal;
                downloadTask.FileDataTransferred = downloadTask.DataTotal;
                await _dbContext.UpdateDownloadFileTransferProgress(key, downloadTask.ToFileTransferProgress());
                fileMergeProgress?.OnNext(downloadTask.ToFileTransferProgress());
            }

            await UpdateDownloadTaskStatus(key, DownloadStatus.MoveFinished);
        }
        catch (OperationCanceledException)
        {
            _log.Here().Warning("The file move operation was cancelled for file task {FileTaskId}", key.Id);

            await UpdateDownloadTaskStatus(key, DownloadStatus.MovePaused);
            await _dbContext.UpdateDownloadFileTransferProgress(key, downloadTask.ToFileTransferProgress());

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _log.Here().Error(ex, "An error occurred while moving file for file task {FileTaskId}", key.Id);
            return await ErrorDownloadTask(key, Result.Fail(new ExceptionalError(ex)));
        }
        finally
        {
            if (_readStream != null)
            {
                await _readStream.DisposeAsync();
                _readStream = null;
            }

            if (_writeStream != null)
            {
                await _writeStream.DisposeAsync();
                _writeStream = null;
            }

            fileMergeProgress?.OnCompleted();
        }

        return Result.Ok();
    }

    private async Task<Result> CopyWithResumeAsync(
        DownloadTaskFileBase downloadTask,
        string sourcePath,
        string targetPath,
        Subject<IDownloadFileTransferProgress>? fileMergeProgress,
        DownloadTaskKey key,
        CancellationToken cancellationToken
    )
    {
        var writeStreamResult = Result.Try(
            (() => _file.Open(targetPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
        );
        if (writeStreamResult.IsFailed)
            return writeStreamResult.ToResult();

        _writeStream = writeStreamResult.Value;

        var inputStreamResult = Result.Try(
            (() => _file.Open(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read))
        );
        if (inputStreamResult.IsFailed)
            return inputStreamResult.ToResult();

        _readStream = inputStreamResult.Value;

        // Resume if needed
        if (downloadTask.CurrentFileTransferBytesOffset > 0)
        {
            _readStream.Seek(downloadTask.CurrentFileTransferBytesOffset, SeekOrigin.Begin);
            _writeStream.Seek(downloadTask.CurrentFileTransferBytesOffset, SeekOrigin.Begin);
        }

        var stopwatch = Stopwatch.StartNew();
        var previousDataTransferred = 0L;

        var buffer = new byte[_bufferSize];
        int bytesRead;
        while ((bytesRead = await _readStream.ReadAsync(buffer, 0, buffer.Length, CancellationToken.None)) > 0)
        {
            await _writeStream.WriteAsync(buffer, 0, bytesRead, CancellationToken.None);

            downloadTask.CurrentFileTransferBytesOffset += bytesRead;
            downloadTask.FileDataTransferred += bytesRead;
            previousDataTransferred += bytesRead;

            downloadTask.FileTransferSpeed = DataFormat.GetTransferSpeed(
                previousDataTransferred,
                stopwatch.Elapsed.TotalSeconds
            );

            fileMergeProgress?.OnNext(downloadTask.ToFileTransferProgress());

            if (stopwatch.ElapsedMilliseconds > 1000)
            {
                _log.Here().Verbose(downloadTask.ToString());

                await _dbContext.UpdateDownloadFileTransferProgress(key, downloadTask.ToFileTransferProgress());
                await _commandExecutor.Send(new DownloadTaskUpdatedCommand(key), CancellationToken.None);

                stopwatch.Restart();
                previousDataTransferred = 0;
            }

            if (cancellationToken.IsCancellationRequested)
            {
                _log.Here()
                    .Information("User Cancellation requested during file copy for file task {FileTaskId}", key.Id);
                break;
            }
        }

        await _readStream.DisposeAsync();
        _readStream = null;
        await _writeStream.DisposeAsync();
        _writeStream = null;

        return Result.Ok();
    }

    private async Task UpdateDownloadTaskStatus(DownloadTaskKey key, DownloadStatus status)
    {
        await _dbContext.SetDownloadStatus(key, status);

        await _commandExecutor.Send(new DownloadTaskUpdatedCommand(key));
    }

    private async Task<Result> ErrorDownloadTask(DownloadTaskKey key, Result result)
    {
        await _dbContext.SetDownloadStatus(key, DownloadStatus.MoveError);

        await _commandExecutor.Send(new DownloadTaskUpdatedCommand(key));

        await _eventPublisher.PublishAsync(new SendNotificationResult(result), CancellationToken.None);

        return result.LogError();
    }
}