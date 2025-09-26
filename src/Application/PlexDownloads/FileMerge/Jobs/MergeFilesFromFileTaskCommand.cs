using System.Diagnostics;
using System.IO.Abstractions;
using System.Reactive.Subjects;
using FastEndpoints;
using FluentValidation;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;

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

    private Stream? _readStream;
    private Stream? _writeStream;

    /// <summary>
    /// Based on https://github.com/dotnet/runtime/discussions/74405#discussioncomment-3488674
    /// 1048576 bytes = 1 MB
    /// </summary>
    private const int _bufferSize = 1048576;

    public MergeFilesFromFileTaskCommandHandler(
        ILogger log,
        ICommandExecutor commandExecutor,
        IEventPublisher eventPublisher,
        IReaparrDbContext dbContext,
        IFile file,
        IDirectory directory,
        IPath path
    )
    {
        _log = log.ForContext<MergeFilesFromFileTaskCommandHandler>();
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

        var sourceFilePath = downloadTask.FilePath;
        _log.Here().Debug("Starting file move process for {FileName}", downloadTask.FileName);

        try
        {
            var directoryPathResult = Result.Try(() => _path.GetDirectoryName(downloadTask.DestinationFilePath));
            if (directoryPathResult.IsFailed)
                return directoryPathResult.ToResult();

            if (string.IsNullOrEmpty(directoryPathResult.Value))
                return Result.Fail($"Could not determine the directory name of path: {directoryPathResult.Value}");

            // Ensure a destination directory exists and is otherwise created.
            var createDirectoryResult = Result
                .Try((() => _directory.CreateDirectory(directoryPathResult.Value)))
                .ToResult();
            if (createDirectoryResult.IsFailed)
                return (await ErrorDownloadTask(downloadTask, createDirectoryResult)).LogError();

            if (string.IsNullOrWhiteSpace(sourceFilePath) || !_file.Exists(sourceFilePath))
            {
                var result = Result
                    .Fail($"Source file does not exist and cannot be moved: {sourceFilePath}")
                    .LogError();
                return await ErrorDownloadTask(downloadTask, result);
            }

            // Update download task status
            downloadTask.DownloadStatus = DownloadStatus.Moving;
            await UpdateDownloadTaskStatus(downloadTask);

            // Try fast move (rename). If a destination exists, delete it first
            Result.Try((() =>
            {
                if (_file.Exists(downloadTask.DestinationFilePath))
                    _file.Delete(downloadTask.DestinationFilePath);
            })).LogIfFailed();

            var moveResult = Result.Try((() => _file.Move(sourceFilePath, downloadTask.DestinationFilePath)));
            if (moveResult.IsFailed)
            {
                // Fallback to copy when a move is not possible (e.g., across filesystems)
                var writeStreamResult = Result.Try(
                    (() => _file.Create(downloadTask.DestinationFilePath, _bufferSize, FileOptions.SequentialScan))
                );
                if (writeStreamResult.IsFailed)
                    return (await ErrorDownloadTask(downloadTask, writeStreamResult.ToResult())).LogError();

                _writeStream = writeStreamResult.Value;

                var inputStreamResult = Result.Try(
                    (() => _file.Open(sourceFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                );
                if (inputStreamResult.IsFailed)
                    return (await ErrorDownloadTask(downloadTask, inputStreamResult.ToResult())).LogError();

                _readStream = inputStreamResult.Value;

                // Resume the file move/copy if it was previously interrupted
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

                    cancellationToken.ThrowIfCancellationRequested();
                }

                await _readStream.DisposeAsync();
                _readStream = null;
                await _writeStream.DisposeAsync();
                _writeStream = null;

                // Copy finished, delete a source temp file
                var deleteResult = Result.Try((() => _file.Delete(sourceFilePath)));
                if (deleteResult.IsFailed)
                    return await ErrorDownloadTask(downloadTask, deleteResult);
            }
            else
            {
                // Move (rename) successful: finish instantly
                downloadTask.CurrentFileTransferBytesOffset = downloadTask.DataTotal;
                downloadTask.FileDataTransferred = downloadTask.DataTotal;
                await _dbContext.UpdateDownloadFileTransferProgress(key, downloadTask.ToFileTransferProgress());
                fileMergeProgress?.OnNext(downloadTask.ToFileTransferProgress());
            }

            downloadTask.DownloadStatus = DownloadStatus.MoveFinished;
            await UpdateDownloadTaskStatus(downloadTask);
        }
        catch (OperationCanceledException)
        {
            _log.Here().Warning("The file move operation was cancelled for file task {FileTaskId}", key.Id);

            downloadTask.DownloadStatus = DownloadStatus.MovePaused;

            await UpdateDownloadTaskStatus(downloadTask);
            await _dbContext.UpdateDownloadFileTransferProgress(key, downloadTask.ToFileTransferProgress());

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return (await ErrorDownloadTask(downloadTask, _log.Here().ErrorResult(ex)));
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

        return result.LogError();
    }
}
