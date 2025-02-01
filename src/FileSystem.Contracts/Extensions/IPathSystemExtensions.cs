using System.IO.Abstractions;
using FluentResults;

namespace FileSystem.Contracts;

public static class IPathExtensions
{
    public static Result<long> GetAvailableSpaceByDirectory(this IPath path, string directory)
    {
        try
        {
            var f = new FileInfo(directory);

            var root = path.GetPathRoot(f.FullName);
            if (string.IsNullOrEmpty(root))
                return Result.Fail($"Could not determine root directory of {directory}");

            var drive = path.FileSystem.DriveInfo.New(root);
            return Result.Ok(drive.AvailableFreeSpace);
        }
        catch (Exception e)
        {
            return Result.Fail(new ExceptionalError(e)).LogError();
        }
    }
}
