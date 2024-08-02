using FileSystem.Contracts;

namespace PlexRipper.FileSystem;

public class DiskSystem : IDiskSystem
{
    private readonly IPathSystem _pathSystem;

    public DiskSystem(IPathSystem pathSystem)
    {
        _pathSystem = pathSystem;
    }

    public Result<long> GetAvailableSpaceByDirectory(string directory)
    {
        try
        {
            var root = _pathSystem.GetPathRoot(directory);
            var drive = new DriveInfo(root);
            return Result.Ok(drive.AvailableFreeSpace);
        }
        catch (Exception e)
        {
            return Result.Fail(new ExceptionalError(e)).LogError();
        }
    }

    public Result HasDirectoryEnoughAvailableSpace(string directory, long fileSize)
    {
        var availableSpaceResult = GetAvailableSpaceByDirectory(directory);
        if (availableSpaceResult.IsFailed)
            return availableSpaceResult.ToResult().LogError();

        if (availableSpaceResult.Value < fileSize)
        {
            return Result.Fail($"There is not enough space available in root directory {directory}");
        }

        return Result.Ok();
    }
}
