using System.IO.Abstractions;
using FileSystem.Contracts;

namespace PlexRipper.FileSystem;

public class DiskSystem : IDiskSystem
{
    private readonly IPath _path;

    public DiskSystem(IPath path)
    {
        _path = path;
    }

    public Result<long> GetAvailableSpaceByDirectory(string directory)
    {
        try
        {
            var f = new FileInfo(directory);

            var root = _path.GetPathRoot(f.FullName);
            if (string.IsNullOrEmpty(root))
                return Result.Fail($"Could not determine root directory of {directory}");

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
            return Result.Fail($"There is not enough space available in root directory {directory}");

        return Result.Ok();
    }
}
