using System.Reactive.Subjects;
using Application.Contracts;
using FileSystem.Contracts;
using PlexRipper.Application;

namespace PlexRipper.FileSystem.Common;

public class FileMergeStreamProvider : IFileMergeStreamProvider
{
    private readonly IFileSystem _fileSystem;

    private readonly INotificationsService _notificationsService;

    private readonly IDirectorySystem _directorySystem;

    private const int _bufferSize = 524288;

    public FileMergeStreamProvider(IFileSystem fileSystem, INotificationsService notificationsService, IDirectorySystem directorySystem)
    {
        _fileSystem = fileSystem;
        _notificationsService = notificationsService;
        _directorySystem = directorySystem;
    }

    public async Task<Result<Stream>> OpenOrCreateMergeStream(string fileDestinationPath)
    {
        // Ensure destination directory exists and is otherwise created.
        var result = _directorySystem.CreateDirectoryFromFilePath(fileDestinationPath);
        if (result.IsFailed)
        {
            await _notificationsService.SendResult(result);
            return result.LogError();
        }

        return _fileSystem.Create(fileDestinationPath, _bufferSize, FileOptions.SequentialScan);
    }

    public async Task MergeFiles(
        List<string> filePaths,
        Stream destination,
        Subject<long> bytesReceivedProgress,
        CancellationToken cancellationToken = default)
    {
        long totalRead = 0;
        foreach (var filePath in filePaths)
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
                    Log.Error("Exception during downloading of ", ex);
                    throw;
                }
            }

            Log.Debug($"The file at {filePath} has been combined into");
            Log.Debug($"Deleting file {filePath} since it has been merged already");
            _fileSystem.DeleteFile(filePath);
        }

        bytesReceivedProgress.OnNext(totalRead);
    }
}