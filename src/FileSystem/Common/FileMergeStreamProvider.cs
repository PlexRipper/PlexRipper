using System.Reactive.Subjects;
using Application.Contracts;
using FileSystem.Contracts;
using Logging.Interface;

namespace PlexRipper.FileSystem;

public class FileMergeStreamProvider : IFileMergeStreamProvider
{
    private readonly ILog _log;
    private readonly IMediator _mediator;
    private readonly IFileSystem _fileSystem;

    private readonly IDirectorySystem _directorySystem;

    private const int _bufferSize = 524288;

    public FileMergeStreamProvider(
        ILog log,
        IMediator mediator,
        IFileSystem fileSystem,
        IDirectorySystem directorySystem
    )
    {
        _log = log;
        _mediator = mediator;
        _fileSystem = fileSystem;
        _directorySystem = directorySystem;
    }

    public async Task<Result<Stream>> OpenOrCreateMergeStream(string fileDestinationPath)
    {
        // Ensure destination directory exists and is otherwise created.
        var result = _directorySystem.CreateDirectoryFromFilePath(fileDestinationPath);
        if (result.IsFailed)
        {
            await _mediator.SendNotificationAsync(result);
            return result.LogError();
        }

        return _fileSystem.Create(fileDestinationPath, _bufferSize, FileOptions.SequentialScan);
    }

    public async Task MergeFiles(
        FileTask fileTask,
        Stream destination,
        Subject<long> bytesReceivedProgress,
        CancellationToken cancellationToken = default
    )
    {
        long totalRead = 0;
        foreach (var filePath in fileTask.FilePaths)
        {
            await using (var inputStream = File.OpenRead(filePath))
            {
                try
                {
                    var buffer = new byte[_bufferSize];
                    int bytesRead;
                    while ((bytesRead = await inputStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                    {
                        await destination.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                        cancellationToken.ThrowIfCancellationRequested();
                        totalRead += bytesRead;
                        bytesReceivedProgress.OnNext(totalRead);
                    }
                }
                catch (Exception ex)
                {
                    _log.Error(ex);
                    throw;
                }
            }

            _log.Here()
                .Debug(
                    "The file at {FilePath} has been merged into the single media file at {DestinationPath}",
                    filePath,
                    fileTask.DestinationFilePath
                );
            _log.Debug("Deleting file {FilePath} since it has been merged already", filePath);
            var deleteResult = _fileSystem.DeleteFile(filePath);
            if (deleteResult.IsFailed)
            {
                await _mediator.SendNotificationAsync(deleteResult);
                deleteResult.LogError();
            }
        }

        bytesReceivedProgress.OnNext(totalRead);
    }
}
