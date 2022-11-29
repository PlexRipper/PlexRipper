using PlexRipper.FileSystem.Common;

namespace PlexRipper.BaseTests;

public class MockFileMergeSystem : IFileMergeSystem
{
    public bool FileExists(string path)
    {
        return true;
    }

    public Result DeleteDirectoryFromFilePath(string path)
    {
        return Result.Ok();
    }

    public Result DeleteAllFilesFromDirectory(string directory)
    {
        return Result.Ok();
    }
}