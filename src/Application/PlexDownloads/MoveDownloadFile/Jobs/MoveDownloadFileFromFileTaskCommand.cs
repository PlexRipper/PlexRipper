using System.Diagnostics;
using System.IO.Abstractions;
using System.Reactive.Subjects;
using FastEndpoints;
using FluentValidation;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;
using Reaparr.FileSystem.Contracts;
using Reaparr.Settings.Contracts;

namespace Reaparr.Application;

public record MoveDownloadFileFromFileTaskCommand(
    DownloadTaskKey Key,
    Subject<IDownloadFileTransferProgress>? FileMergeProgress = null
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
    private readonly Serilog.ILogger _log;
    private readonly ICommandExecutor _commandExecutor;
    private readonly IEventPublisher _eventPublisher;
    private readonly IReaparrDbContext _dbContext;
    private readonly IFile _file;
    private readonly IDirectory _directory;
    private readonly IPath _path;
    private readonly IDownloadManagerSettings _downloadManagerSettings;

    public MoveDownloadFileFromFileTaskCommandHandler(
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
        _log = log.ForContext<MoveDownloadFileFromFileTaskCommandHandler>();
        _commandExecutor = commandExecutor;
        _eventPublisher = eventPublisher;
        _dbContext = dbContext;
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
        var fileMergeProgress = command.FileMergeProgress;

        var downloadTask = await _dbContext.GetDownloadTaskFileAsync(command.Key, CancellationToken.None);
        if (downloadTask == null)
            return ResultExtensions.EntityNotFound(nameof(DownloadTaskGeneric), command.Key.Id).LogError();

        // Resolve paths
        var downloadFilePath = downloadTask.FilePath;
        var destinationPath = downloadTask.DestinationFilePath;

        _log.Here().Debug("Starting file move process for {DownloadFilePath}", downloadFilePath);

        if (string.IsNullOrWhiteSpace(downloadFilePath) || !_file.Exists(downloadFilePath))
        {
            var result = Result.Fail($"Source file does not exist and cannot be moved: {downloadFilePath}").LogError();
            return await ErrorDownloadTask(key, result);
        }

        try
        {
            if (downloadFilePath.RemoveReapTempSuffix() == destinationPath)
            {
                // Just rename it to remove .reapTemp suffix if present
                _file.Move(downloadFilePath.RemoveReapTempSuffix(), destinationPath);

                downloadTask.CurrentFileTransferBytesOffset = downloadTask.DataTotal;
                downloadTask.FileDataTransferred = downloadTask.DataTotal;
                await _dbContext.UpdateDownloadFileTransferProgress(key, downloadTask.ToFileTransferProgress());
                fileMergeProgress?.OnNext(downloadTask.ToFileTransferProgress());

                await UpdateDownloadTaskStatus(key, DownloadStatus.MoveFinished);
                return Result.Ok();
            }

            // Determine if we should keep the file in the downloads directory
            var destinationDirectoryPath = _path.GetDirectoryName(destinationPath);
            var keepInDownloads = ShouldKeepInDownloads(downloadTask);

            if (keepInDownloads)
            {
                // Rename it to remove .reapTemp suffix if present
                _file.Move(downloadFilePath, downloadFilePath.RemoveReapTempSuffix());

                downloadTask.CurrentFileTransferBytesOffset = downloadTask.DataTotal;
                downloadTask.FileDataTransferred = downloadTask.DataTotal;
                await _dbContext.UpdateDownloadFileTransferProgress(key, downloadTask.ToFileTransferProgress());
                fileMergeProgress?.OnNext(downloadTask.ToFileTransferProgress());

                await UpdateDownloadTaskStatus(key, DownloadStatus.MoveFinished);
                return Result.Ok();
            }

            // Ensure destination directory exists
            var directoryPathResult = Result.Try(() => _path.GetDirectoryName(destinationPath));
            if (directoryPathResult.IsFailed)
                return await ErrorDownloadTask(key, directoryPathResult.ToResult());

            if (string.IsNullOrEmpty(directoryPathResult.Value))
                return await ErrorDownloadTask(key,
                    Result.Fail($"Could not determine the directory name of path: {directoryPathResult.Value}"));

            // Ensure the destination directory exists only when we are actually moving
            var createDirectoryResult = Result
                .Try((() => _directory.CreateDirectory(destinationDirectoryPath!)))
                .ToResult();
            if (createDirectoryResult.IsFailed)
                return await ErrorDownloadTask(key, createDirectoryResult);

            // Update status before moving
            await UpdateDownloadTaskStatus(key, DownloadStatus.Moving);

            var moveResult = await MoveWithResumeAsync(
                downloadTask,
                downloadFilePath,
                destinationPath,
                key,
                cancellationToken
            );
            if (moveResult.IsFailed)
                return await ErrorDownloadTask(key, moveResult);

            // Instant finish on rename
            downloadTask.CurrentFileTransferBytesOffset = downloadTask.DataTotal;
            downloadTask.FileDataTransferred = downloadTask.DataTotal;
            await _dbContext.UpdateDownloadFileTransferProgress(key, downloadTask.ToFileTransferProgress());
            fileMergeProgress?.OnNext(downloadTask.ToFileTransferProgress());

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
            fileMergeProgress?.OnCompleted();
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
        var stopwatch = Stopwatch.StartNew();

        void SendProgress(MoveFileTransferProgressDTO progress)
        {
            downloadTask.CurrentFileTransferBytesOffset = progress.Transferred;
            downloadTask.FileDataTransferred = progress.Transferred;
            downloadTask.FileTransferSpeed = progress.FileTransferSpeed;

            if (stopwatch.ElapsedMilliseconds > 1000)
            {
                _log.Here().Verbose(downloadTask.ToString());

                var fileTransferProgress = downloadTask.ToFileTransferProgress();

                _ = Task.Run(
                    async () =>
                    {
                        try
                        {
                            await _dbContext.UpdateDownloadFileTransferProgress(key, fileTransferProgress);
                            await _commandExecutor.Send(new DownloadTaskUpdatedCommand(key), CancellationToken.None);
                        }
                        catch (Exception ex)
                        {
                            _log.Here()
                                .Error(ex, "Error while updating file transfer progress for {FileTaskId}", key.Id);
                        }
                    },
                    cancellationToken
                );

                stopwatch.Restart();
            }
        }

        return await _commandExecutor.Send(
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

    private bool ShouldKeepInDownloads(DownloadTaskFileBase downloadTask) =>
        downloadTask.DirectoryMeta.KeepCompletedInDownloadFolder
        || _downloadManagerSettings.KeepCompletedInDownloadFolder
        || downloadTask.FilePath == downloadTask.DestinationFilePath
        || string.IsNullOrWhiteSpace(downloadTask.DestinationFilePath);
}