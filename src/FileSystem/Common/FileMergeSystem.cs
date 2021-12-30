using FluentResults;
using PlexRipper.Application;

namespace PlexRipper.FileSystem.Common
{
    /// <summary>
    /// This is used to mock all I/O dependancies for the <see cref="FileMerger"/> class.
    /// </summary>
    public class FileMergeSystem : IFileMergeSystem
    {
        private readonly IFileSystem _fileSystem;

        public FileMergeSystem(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public bool FileExists(string path)
        {
            return _fileSystem.FileExists(path);
        }

        public Result DeleteDirectoryFromFilePath(string path)
        {
            return _fileSystem.DeleteDirectoryFromFilePath(path);
        }

        public Result DeleteAllFilesFromDirectory(string directory)
        {
            return _fileSystem.DeleteAllFilesFromDirectory(directory);
        }
    }
}