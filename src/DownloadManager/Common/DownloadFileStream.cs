using System;
using System.IO;
using FluentResults;
using PlexRipper.Application;

namespace PlexRipper.DownloadManager
{
    public class DownloadFileStream : IDownloadFileStream
    {
        private readonly IFileSystem _fileSystem;

        private readonly IDirectorySystem _directorySystem;

        private readonly IPathSystem _pathSystem;

        private readonly IDiskSystem _diskSystem;

        public DownloadFileStream(IFileSystem fileSystem, IDirectorySystem directorySystem, IPathSystem pathSystem, IDiskSystem diskSystem)
        {
            _fileSystem = fileSystem;
            _directorySystem = directorySystem;
            _pathSystem = pathSystem;
            _diskSystem = diskSystem;
        }

        public Result<Stream> CreateDownloadFileStream(string directory, string fileName, long fileSize)
        {
            try
            {
                var createDirectoryResult = _directorySystem.CreateDirectory(directory);
                if (createDirectoryResult.IsFailed)
                    return createDirectoryResult.ToResult();

                // TODO This might need to be determined sooner, like when adding downloadTasks
                var availableSpace = _diskSystem.GetAvailableSpaceByDirectory(directory);
                if (availableSpace.IsFailed)
                    return availableSpace.ToResult();

                if (availableSpace.Value < fileSize)
                    return Result.Fail($"There is not enough space available in root directory {directory}");

                var filePath = _pathSystem.Combine(directory, fileName);
                if (filePath.IsFailed)
                {
                    return filePath.ToResult();
                }

                Stream fileStream;
                if (_fileSystem.FileExists(filePath.Value))
                {
                    var openResult = _fileSystem.Open(filePath.Value, FileMode.Open, FileAccess.ReadWrite, FileShare.Delete);
                    if (openResult.IsFailed)
                    {
                        return openResult.ToResult().LogError();
                    }

                    fileStream = openResult.Value;
                }
                else
                {
                    var createResult = _fileSystem.Create(filePath.Value, 2048, FileOptions.Asynchronous);
                    if (createResult.IsFailed)
                    {
                        return createResult.ToResult().LogError();
                    }

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
}