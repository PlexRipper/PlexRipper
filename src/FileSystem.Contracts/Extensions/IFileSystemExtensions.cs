using System.IO.Abstractions;
using FluentResults;

namespace FileSystem.Contracts;

public static class IFileSystemExtensions
{
    public static Result<long> GetAvailableSpaceByDirectory(this IFileSystem fileSystem, string directory)
    {
        try
        {
            var f = new FileInfo(directory);

            var root = fileSystem.Path.GetPathRoot(f.FullName);
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
}
