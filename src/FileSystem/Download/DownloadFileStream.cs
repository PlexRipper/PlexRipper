using System.IO.Abstractions;
using FileSystem.Contracts;

namespace PlexRipper.FileSystem;

public class DownloadFileStream : IDownloadFileStream
{
    private readonly IDirectorySystem _directorySystem;

    private readonly IDiskSystem _diskSystem;

    private readonly IFileSystem _abstractedFileSystem;

    public DownloadFileStream(
        IDirectorySystem directorySystem,
        IDiskSystem diskSystem,
        IFileSystem abstractedFileSystem
    )
    {
        _directorySystem = directorySystem;
        _diskSystem = diskSystem;
        _abstractedFileSystem = abstractedFileSystem;
    }

    public Result<Stream> CreateDownloadFileStream(string directory, string fileName, long fileSize)
    {
        try
        {
            var createDirectoryResult = _directorySystem.CreateDirectory(directory);
            if (createDirectoryResult.IsFailed)
                return createDirectoryResult.ToResult();

            // TODO:This might need to be determined sooner, like when adding downloadTasks
            var availableSpace = _diskSystem.GetAvailableSpaceByDirectory(directory);
            if (availableSpace.IsFailed)
                return availableSpace.ToResult();

            if (availableSpace.Value < fileSize)
                return Result.Fail($"There is not enough space available in root directory {directory}");

            var combineResult = Result.Try((() => _abstractedFileSystem.Path.Combine(directory, fileName)));
            if (combineResult.IsFailed)
                return combineResult.ToResult();

            var filePath = combineResult.Value;
            Stream fileStream;
            if (_abstractedFileSystem.File.Exists(filePath))
            {
                var openResult = Result.Try(
                    () =>
                        _abstractedFileSystem.File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.Delete)
                );
                if (openResult.IsFailed)
                    return openResult.ToResult().LogError();

                fileStream = openResult.Value;
            }
            else
            {
                var createResult = Result.Try(
                    () => _abstractedFileSystem.File.Create(filePath, 2048, FileOptions.Asynchronous)
                );
                if (createResult.IsFailed)
                    return createResult.ToResult().LogError();

                fileStream = createResult.Value;
            }

            // Pre-allocate the required file size
            fileStream.SetLength(fileSize);
            return Result.Ok(fileStream);
        }
        catch (Exception e)
        {
            return Result.Fail(new ExceptionalError(e)).LogError();
        }
    }
}
