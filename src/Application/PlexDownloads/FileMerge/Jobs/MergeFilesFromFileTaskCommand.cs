using System.Diagnostics;
using System.IO.Abstractions;
using System.Reactive.Subjects;
using Application.Contracts;
using Data.Contracts;
using FastEndpoints;
using FluentValidation;
using Logging.Interface;

namespace PlexRipper.Application;

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
    private readonly ILog _log;
    private readonly ICommandExecutor _commandExecutor;
    private readonly IEventPublisher _eventPublisher;
    private readonly IPlexRipperDbContext _dbContext;
    private readonly IFile _file;
    private readonly IDirectory _directory;
    private readonly IPath _path;

    private Stream? _readStream;
    private Stream? _writeStream;

    /// <summary>
    /// Based on https://github.com/dotnet/runtime/discussions/74405#discussioncomment-3488674
    /// 1048576 bytes = 1 MB
    /// </summary>
    private const int _bufferSize = 1048576;

    public MergeFilesFromFileTaskCommandHandler(
        ILog log,
        ICommandExecutor commandExecutor,
        IEventPublisher eventPublisher,
        IPlexRipperDbContext dbContext,
        IFile file,
        IDirectory directory,
        IPath path
    )
    {
        _log = log;
        _commandExecutor = commandExecutor;
        _eventPublisher = eventPublisher;
        _dbContext = dbContext;
        _file = file;
        _directory = directory;
        _path = path;
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

        var sourceFilePaths = downloadTask.FilePaths;
        _log.Here()
            .Debug(
                "Starting file merge process for {FilePathsCount} parts into a file {FileName}",
                sourceFilePaths.Count,
                downloadTask.FileName
            );

        try
        {
            var directoryPathResult = Result.Try(() => _path.GetDirectoryName(downloadTask.DestinationFilePath));
            if (directoryPathResult.IsFailed)
                return directoryPathResult.ToResult();

            if (string.IsNullOrEmpty(directoryPathResult.Value))
                return Result.Fail($"Could not determine the directory name of path: {directoryPathResult.Value}");

            // Ensure destination directory exists and is otherwise created.
            var createDirectoryResult = Result
                .Try((() => _directory.CreateDirectory(directoryPathResult.Value)))
                .ToResult();
            if (createDirectoryResult.IsFailed)
                return (await ErrorDownloadTask(downloadTask, createDirectoryResult)).LogError();

            var writeStreamResult = Result.Try(
                (() => _file.Create(downloadTask.DestinationFilePath, _bufferSize, FileOptions.SequentialScan))
            );
            if (writeStreamResult.IsFailed)
                return (await ErrorDownloadTask(downloadTask, writeStreamResult.ToResult())).LogError();

            _writeStream = writeStreamResult.Value;

            // Resume the file merge if it was previously interrupted
            if (downloadTask.CurrentFileTransferBytesOffset > 0)
            {
                _writeStream.Seek(downloadTask.CurrentFileTransferBytesOffset, SeekOrigin.Begin);
            }

            // Update download task status
            downloadTask.DownloadStatus = downloadTask.IsSingleFile ? DownloadStatus.Moving : DownloadStatus.Merging;
            await UpdateDownloadTaskStatus(downloadTask);

            var stopwatch = Stopwatch.StartNew(); // Start timing for speed calculation
            var previousDataTransferred = downloadTask.FileDataTransferred;

            for (var index = downloadTask.CurrentFileTransferPathIndex; index < sourceFilePaths.Count; index++)
            {
                var filePath = sourceFilePaths[index];

                if (!_file.Exists(filePath))
                {
                    var result = Result
                        .Fail($"Filepath: {filePath} does not exist and cannot be used to merge/move the file!")
                        .LogError();

                    return (await ErrorDownloadTask(downloadTask, result)).LogError();
                }

                var inputStreamResult = Result.Try(
                    (() => _file.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                );
                if (inputStreamResult.IsFailed)
                    return (await ErrorDownloadTask(downloadTask, inputStreamResult.ToResult())).LogError();

                _readStream = inputStreamResult.Value;

                if (downloadTask.CurrentFileTransferBytesOffset > 0)
                {
                    _readStream.Seek(downloadTask.CurrentFileTransferBytesOffset, SeekOrigin.Begin);
                }

                downloadTask.CurrentFileTransferPathIndex = index;
                downloadTask.CurrentFileTransferBytesOffset = 0;

                cancellationToken.ThrowIfCancellationRequested();

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

                    // Send progress
                    fileMergeProgress?.OnNext(downloadTask.ToFileTransferProgress());

                    if (stopwatch.ElapsedMilliseconds > 1000)
                    {
                        _log.VerboseLine(downloadTask.ToString());

                        await _dbContext.UpdateDownloadFileTransferProgress(key, downloadTask.ToFileTransferProgress());
                        await _commandExecutor.Send(new DownloadTaskUpdatedCommand(key), CancellationToken.None);

                        stopwatch.Restart();
                        previousDataTransferred = 0;
                    }

                    cancellationToken.ThrowIfCancellationRequested();
                }

                _log.Here()
                    .Debug(
                        "The file at {FilePath} has been merged into the single media file at {DestinationPath}",
                        filePath,
                        downloadTask.DestinationDirectory
                    );

                // Important: Reset the offset between files otherwise it skips parts of the file
                downloadTask.CurrentFileTransferBytesOffset = 0;

                _log.Debug("Deleting file {FilePath} since it has been merged already", filePath);
                await _readStream.DisposeAsync();
                _readStream = null;

                var deleteResult = Result.Try((() => _file.Delete(filePath)));
                if (deleteResult.IsFailed)
                    return (await ErrorDownloadTask(downloadTask, deleteResult)).LogError();
            }

            // Important: Reset the offset between files otherwise it skips parts of the file
            downloadTask.CurrentFileTransferBytesOffset = 0;

            _log.Here()
                .Information(
                    "Finished combining {FilePathsCount} files into {FileTaskFileName}",
                    downloadTask.FilePaths.Count,
                    downloadTask.FileName
                );

            await _dbContext.UpdateDownloadFileTransferProgress(key, downloadTask.ToFileTransferProgress());
            fileMergeProgress?.OnNext(downloadTask.ToFileTransferProgress());

            downloadTask.DownloadStatus = downloadTask.IsSingleFile
                ? DownloadStatus.MoveFinished
                : DownloadStatus.MergeFinished;

            await UpdateDownloadTaskStatus(downloadTask);
        }
        catch (OperationCanceledException)
        {
            _log.Warning("The file merge operation was cancelled for file task {FileTaskId}", key.Id);

            downloadTask.DownloadStatus = downloadTask.IsSingleFile
                ? DownloadStatus.MovePaused
                : DownloadStatus.MergePaused;

            await UpdateDownloadTaskStatus(downloadTask);
            await _dbContext.UpdateDownloadFileTransferProgress(key, downloadTask.ToFileTransferProgress());

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return (await ErrorDownloadTask(downloadTask, _log.Error(ex).ToResult()));
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

    private async Task UpdateDownloadTaskStatus(DownloadTaskFileBase downloadTask)
    {
        await _dbContext.SetDownloadStatus(downloadTask.ToKey(), downloadTask.DownloadStatus);

        await _commandExecutor.Send(new DownloadTaskUpdatedCommand(downloadTask.ToKey()));
    }

    private async Task<Result> ErrorDownloadTask(DownloadTaskFileBase downloadTask, Result result)
    {
        downloadTask.DownloadStatus = downloadTask.IsSingleFile ? DownloadStatus.MoveError : DownloadStatus.MergeError;

        await _dbContext.SetDownloadStatus(downloadTask.ToKey(), downloadTask.DownloadStatus);

        await _commandExecutor.Send(new DownloadTaskUpdatedCommand(downloadTask.ToKey()));

        await _eventPublisher.PublishAsync(new SendNotificationResult(result), CancellationToken.None);

        return result;
    }
}
