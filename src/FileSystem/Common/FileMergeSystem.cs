using FileSystem.Contracts;

namespace PlexRipper.FileSystem;

public class FileMergeSystem : IFileMergeSystem
{
    private readonly IFileSystem _fileSystem;

    private readonly IDirectorySystem _directorySystem;

    public FileMergeSystem(IFileSystem fileSystem, IDirectorySystem directorySystem)
    {
        _fileSystem = fileSystem;
        _directorySystem = directorySystem;
    }

    public bool FileExists(string path) => _fileSystem.FileExists(path);

    public Result DeleteDirectoryFromFilePath(string path) => _directorySystem.DeleteDirectoryFromFilePath(path);

    public Result DeleteAllFilesFromDirectory(string directory) =>
        _directorySystem.DeleteAllFilesFromDirectory(directory);
}