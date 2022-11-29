using PlexRipper.Application;

namespace PlexRipper.FileSystem.Common;

/// <summary>
/// This is used to mock all I/O dependencies for the <see cref="FileMerger"/> class.
/// </summary>
public class FileMergeSystem : IFileMergeSystem
{
    private readonly IFileSystem _fileSystem;

    private readonly IDirectorySystem _directorySystem;

    public FileMergeSystem(IFileSystem fileSystem, IDirectorySystem directorySystem)
    {
        _fileSystem = fileSystem;
        _directorySystem = directorySystem;
    }

    public bool FileExists(string path)
    {
        return _fileSystem.FileExists(path);
    }

    public Result DeleteDirectoryFromFilePath(string path)
    {
        return _directorySystem.DeleteDirectoryFromFilePath(path);
    }

    public Result DeleteAllFilesFromDirectory(string directory)
    {
        return _directorySystem.DeleteAllFilesFromDirectory(directory);
    }
}