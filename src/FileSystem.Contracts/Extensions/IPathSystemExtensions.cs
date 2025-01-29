using System.IO.Abstractions;
using FluentResults;

namespace FileSystem.Contracts;

public static class IPathExtensions
{
    public static Result<long> GetAvailableSpaceByDirectory(this IPath path, string directory)
    {
        try
        {
            var drive = path.FileSystem.DriveInfo.New(directory);
            return Result.Ok(drive.AvailableFreeSpace);
        }
        catch (Exception e)
        {
            return Result.Fail(new ExceptionalError(e)).LogError();
        }
    }
}
