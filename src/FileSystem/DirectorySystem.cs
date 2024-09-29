using FileSystem.Contracts;
using Logging.Interface;

namespace PlexRipper.FileSystem;

public class DirectorySystem : IDirectorySystem
{
    private readonly ILog _log;
    private readonly IPathSystem _pathSystem;

    public DirectorySystem(ILog<DirectorySystem> log, IPathSystem pathSystem)
    {
        _log = log;
        _pathSystem = pathSystem;
    }

    /// <inheritdoc />
    public Result<bool> Exists(string path)
    {
        try
        {
            if (path == string.Empty)
                return Result.Ok(false);

            var di = new DirectoryInfo(path);
            return Result.Ok(di.Exists);
        }
        catch (Exception e)
        {
            return Result.Fail(new ExceptionalError(e)).LogError();
        }
    }

    public Result<DirectoryInfo> CreateDirectory(string path)
    {
        try
        {
            return Result.Ok(Directory.CreateDirectory(path));
        }
        catch (Exception e)
        {
            return Result.Fail(new ExceptionalError(e)).LogError();
        }
    }

    public Result CreateDirectoryFromFilePath(string filePath)
    {
        if (string.IsNullOrEmpty(filePath) || string.IsNullOrWhiteSpace(filePath))
            return Result.Fail("Parameter filepath was empty");

        // TODO this fails with: /mnt/DATA/PlexRipperCache/Downloads/TvShows/Reno 911!/Season 1/Reno 911! - S01E01 - How We Do It in Reno (Pilot) WEBDL-1080p.part1.mkv
        var directoryPathResult = _pathSystem.GetDirectoryName(filePath);
        if (directoryPathResult.IsFailed)
            return directoryPathResult.ToResult();

        if (string.IsNullOrEmpty(directoryPathResult.Value))
            return Result.Fail($"Could not determine the directory name of path: {filePath}");

        return CreateDirectory(directoryPathResult.Value).ToResult();
    }

    /// <inheritdoc />
    public Result DeleteAllFilesFromDirectory(string directory)
    {
        try
        {
            var directoryExistsResult = Exists(directory);
            if (directoryExistsResult.IsFailed)
                return directoryExistsResult.ToResult();

            if (!directoryExistsResult.Value)
            {
                _log.Debug(
                    "Directory: {Directory} does not exist so the files could not be deleted from it",
                    directory
                );
                return Result.Ok();
            }

            var di = new DirectoryInfo(directory);

            foreach (var file in di.GetFiles())
                file.Delete();

            foreach (var dir in di.GetDirectories())
                dir.Delete(true);

            return Result.Ok();
        }
        catch (Exception e)
        {
            return Result.Fail(new ExceptionalError(e)).LogError();
        }
    }

    public Result<string[]> GetFiles(string directoryPath)
    {
        try
        {
            return Result.Ok(Directory.GetFiles(directoryPath));
        }
        catch (Exception e)
        {
            return Result.Fail(new ExceptionalError(e)).LogError();
        }
    }

    public Result Delete(string directoryPath)
    {
        try
        {
            Directory.Delete(directoryPath);
            return Result.Ok();
        }
        catch (Exception e)
        {
            return Result.Fail(new ExceptionalError(e)).LogError();
        }
    }

    /// <inheritdoc />
    public Result DeleteDirectoryFromFilePath(string filePath)
    {
        if (string.IsNullOrEmpty(filePath) || string.IsNullOrWhiteSpace(filePath))
            return Result.Fail("Parameter filepath was empty").LogError();

        try
        {
            var directoryResult = _pathSystem.GetDirectoryName(filePath);
            if (directoryResult.IsFailed)
                return directoryResult.ToResult();

            var directoryExistsResult = Exists(directoryResult.Value);
            if (directoryExistsResult.IsFailed)
                return directoryExistsResult.ToResult();

            var directoryHasFiles = GetFiles(directoryResult.Value);
            if (directoryHasFiles.IsFailed)
                return directoryHasFiles.ToResult();

            // If the filePath is just an empty directory then delete that.
            if (
                !string.IsNullOrEmpty(directoryResult.Value)
                && directoryExistsResult.Value
                && !directoryHasFiles.Value.Any()
            )
                Delete(directoryResult.Value);
            else
            {
                return Result
                    .Fail($"Could not determine the directory name of path: {filePath} or the path contains files")
                    .LogError();
            }
        }
        catch (Exception e)
        {
            return Result.Fail(new ExceptionalError(e)).LogError();
        }

        return Result.Ok();
    }
}
