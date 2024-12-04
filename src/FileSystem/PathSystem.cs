using System.IO.Abstractions;
using FileSystem.Contracts;

namespace PlexRipper.FileSystem;

public class PathSystem : IPathSystem
{
    private readonly IPath _path;

    public PathSystem(IPath path)
    {
        _path = path;
    }

    public Result<string> Combine(params string[] paths)
    {
        try
        {
            return Result.Ok(_path.Combine(paths));
        }
        catch (Exception e)
        {
            return Result.Fail(new ExceptionalError(e)).LogError();
        }
    }

    /// <inheritdoc/>
    public Result<string> GetDirectoryName(string filePath)
    {
        try
        {
            return Result.Ok(_path.GetDirectoryName(filePath) ?? string.Empty);
        }
        catch (Exception e)
        {
            return Result.Fail(new ExceptionalError(e)).LogError();
        }
    }
}
