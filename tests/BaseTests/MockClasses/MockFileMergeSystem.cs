using FileSystem.Contracts;

namespace PlexRipper.BaseTests;

public class MockFileMergeSystem : IFileMergeSystem
{
    public bool FileExists(string path) => true;

    public Result DeleteDirectoryFromFilePath(string path) => Result.Ok();

    public Result DeleteAllFilesFromDirectory(string directory) => Result.Ok();
}