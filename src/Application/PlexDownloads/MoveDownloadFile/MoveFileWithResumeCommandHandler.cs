using System.Diagnostics;
using System.IO.Abstractions;
using FastEndpoints;
using FluentValidation;

namespace Reaparr.Application;

public record MoveFileWithResumeCommand : ICommand<Result>
{
    public required string SourcePath { get; init; }

    public required string TargetPath { get; init; }

    public required long CurrentOffset { get; init; }

    public required long DataTotal { get; init; }

    public required Action<MoveFileTransferProgressDTO> Progress { get; init; }
}

public record MoveFileTransferProgressDTO
{
    /// <summary>
    /// Gets or sets the total size received of the file in bytes.
    /// </summary>
    public required long Transferred { get; init; }

    /// <summary>
    /// Gets or sets the total size of the file in bytes.
    /// </summary>
    public required long DataTotal { get; init; }

    /// <summary>
    /// Gets or sets the file transfer speeds.
    /// </summary>
    public required long FileTransferSpeed { get; init; }
}

public class MoveFileWithResumeValidator : AbstractValidator<MoveFileWithResumeCommand>
{
    public MoveFileWithResumeValidator()
    {
        RuleFor(x => x.SourcePath).NotEmpty();
        RuleFor(x => x.TargetPath).NotEmpty();
        RuleFor(x => x.DataTotal).GreaterThan(0);
    }
}

public class MoveFileWithResumeCommandHandler : ICommandHandler<MoveFileWithResumeCommand, Result>
{
    private readonly ILogger _log;
    private readonly IFile _file;

    /// <summary>
    /// Based on https://github.com/dotnet/runtime/discussions/74405#discussioncomment-3488674
    /// 1048576 bytes = 1 MB
    /// </summary>
    private readonly int _bufferSize = 1048576;

    public MoveFileWithResumeCommandHandler(ILogger log, IFile file)
    {
        _log = log.ForContext<MoveFileWithResumeCommandHandler>();
        _file = file;
    }

    public async Task<Result> ExecuteAsync(MoveFileWithResumeCommand command, CancellationToken cancellationToken)
    {
        var sourcePath = command.SourcePath;
        var targetPath = command.TargetPath;
        var currentOffset = command.CurrentOffset;
        var dataTotal = command.DataTotal;
        var moveDownloadFileProgres = command.Progress;

        var writeStreamResult = Result.Try(() =>
            _file.Open(targetPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite)
        );
        if (writeStreamResult.IsFailed)
            return writeStreamResult.ToResult();

        var inputStreamResult = Result.Try(
            (() => _file.Open(sourcePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        );
        if (inputStreamResult.IsFailed)
            return inputStreamResult.ToResult();

        await using Stream? writeStream = writeStreamResult.Value;
        await using Stream? readStream = inputStreamResult.Value;

        // Resume if needed
        if (currentOffset > 0)
        {
            readStream.Seek(currentOffset, SeekOrigin.Begin);
            writeStream.Seek(currentOffset, SeekOrigin.Begin);
        }

        var stopwatch = Stopwatch.StartNew();
        var previousDataTransferred = 0L;

        var buffer = new byte[_bufferSize];
        int bytesRead;
        while ((bytesRead = await readStream.ReadAsync(buffer, 0, buffer.Length, CancellationToken.None)) > 0)
        {
            await writeStream.WriteAsync(buffer, 0, bytesRead, CancellationToken.None);

            currentOffset += bytesRead;
            previousDataTransferred += bytesRead;

            moveDownloadFileProgres(
                new MoveFileTransferProgressDTO
                {
                    Transferred = currentOffset,
                    DataTotal = dataTotal,
                    FileTransferSpeed = DataFormat.GetTransferSpeed(
                        previousDataTransferred,
                        stopwatch.Elapsed.TotalSeconds
                    ),
                }
            );

            if (cancellationToken.IsCancellationRequested)
            {
                _log.Here()
                    .Warning(
                        "User Cancellation requested during file move form {SourcePath} to {TargetPath}",
                        sourcePath,
                        targetPath
                    );
                break;
            }
        }

        // Only after a successful completion, delete the source file if it still exists
        if (!cancellationToken.IsCancellationRequested && _file.Exists(sourcePath))
        {
            Result.Try(() => _file.Delete(sourcePath)).LogIfFailed();
        }

        return Result.Ok();
    }
}
