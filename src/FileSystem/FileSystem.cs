using Environment;
using FileSystem.Contracts;

namespace PlexRipper.FileSystem;

public class FileSystem : IFileSystem
{
    #region Fields

    private readonly IPathProvider _pathProvider;

    private readonly System.IO.Abstractions.IFileSystem _abstractedFileSystem;

    private readonly IDiskProvider _diskProvider;

    private readonly IDiskSystem _diskSystem;

    private readonly IDirectorySystem _directorySystem;

    #endregion

    #region Constructor

    public FileSystem(
        IPathProvider pathProvider,
        System.IO.Abstractions.IFileSystem abstractedFileSystem,
        IDiskProvider diskProvider,
        IDiskSystem diskSystem,
        IDirectorySystem directorySystem)
    {
        _pathProvider = pathProvider;
        _abstractedFileSystem = abstractedFileSystem;
        _diskProvider = diskProvider;
        _diskSystem = diskSystem;
        _directorySystem = directorySystem;
    }

    #endregion

    #region Public Methods

    public bool FileExists(string path)
    {
        return !string.IsNullOrEmpty(path) && _abstractedFileSystem.File.Exists(path);
    }

    public Result<Stream> Open(string path, FileMode mode, FileAccess access, FileShare share)
    {
        try
        {
            return Result.Ok(_abstractedFileSystem.File.Open(path, mode, access, share));
        }
        catch (Exception e)
        {
            return Result.Fail(new ExceptionalError(e)).LogError();
        }
    }

    public Result<Stream> Create(string path, int bufferSize, FileOptions options)
    {
        try
        {
            var createResult = _abstractedFileSystem.File.Create(path, bufferSize, options);
            return Result.Ok(createResult);
        }
        catch (Exception e)
        {
            return Result.Fail(new ExceptionalError(e)).LogError();
        }
    }

    public Result<string> FileReadAllText(string path)
    {
        if (string.IsNullOrEmpty(path))
            return Result.Fail($"path is empty: \"{path}\"");

        try
        {
            var text = _abstractedFileSystem.File.ReadAllText(path);
            return Result.Ok(text);
        }
        catch (Exception e)
        {
            return Result.Fail(new ExceptionalError(e));
        }
    }

    public Result FileWriteAllText(string path, string text)
    {
        if (string.IsNullOrEmpty(path))
            return Result.Fail($"path is empty: \"{path}\"");

        try
        {
            _abstractedFileSystem.File.WriteAllText(path, text);
            return Result.Ok();
        }
        catch (Exception e)
        {
            return Result.Fail(new ExceptionalError(e));
        }
    }

    public Result DeleteFile(string filePath)
    {
        try
        {
            _abstractedFileSystem.File.Delete(filePath);
            return Result.Ok();
        }
        catch (Exception e)
        {
            return Result.Fail(new ExceptionalError(e)).LogError();
        }
    }

    public Result<FileSystemResult> LookupContents(string query, bool includeFiles, bool allowFoldersWithoutTrailingSlashes)
    {
        Log.Debug($"Looking up path: {query}");
        var directoryExistsResult = _directorySystem.Exists(query);
        if (directoryExistsResult.IsFailed)
            return directoryExistsResult.ToResult();

        // If path is invalid return root file system
        if (string.IsNullOrWhiteSpace(query) || !directoryExistsResult.Value)
        {
            return Result.Ok(new FileSystemResult
            {
                Directories = GetDrives(),
            });
        }

        if (allowFoldersWithoutTrailingSlashes)
            return Result.Ok(GetResult(query, includeFiles));

        var lastSeparatorIndex = query.LastIndexOf(_abstractedFileSystem.Path.DirectorySeparatorChar);
        var path = query.Substring(0, lastSeparatorIndex + 1);

        if (lastSeparatorIndex != -1)
            return Result.Ok(GetResult(path, includeFiles));

        return Result.Ok(new FileSystemResult());
    }

    public string ToAbsolutePath(string relativePath)
    {
        return _abstractedFileSystem.Path.GetFullPath(_abstractedFileSystem.Path.Combine(_pathProvider.RootDirectory, relativePath));
    }

    public Result FileMove(string sourceFileName, string destFileName, bool overwrite = true)
    {
        try
        {
            _abstractedFileSystem.File.Move(sourceFileName, destFileName, overwrite);
            return Result.Ok();
        }
        catch (Exception e)
        {
            return Result.Fail(new ExceptionalError(e)).LogError();
        }
    }

    #endregion

    #region Private Methods

    private List<FileSystemModel> GetDrives()
    {
        return _diskProvider.GetMounts()
            .Select(d => new FileSystemModel
            {
                Type = FileSystemEntityType.Drive,
                Name = _diskProvider.GetVolumeName(d),
                Path = d.RootDirectory,
                LastModified = null,
            })
            .ToList();
    }

    private FileSystemResult GetResult(string path, bool includeFiles)
    {
        var result = new FileSystemResult();

        try
        {
            result.Parent = _diskProvider.GetParent(path);
            result.Directories = _diskProvider.GetDirectories(path);

            if (includeFiles)
                result.Files = _diskProvider.GetFiles(path);
        }
        catch (DirectoryNotFoundException)
        {
            return new FileSystemResult { Parent = _diskProvider.GetParent(path) };
        }
        catch (ArgumentException)
        {
            return new FileSystemResult();
        }
        catch (IOException)
        {
            return new FileSystemResult { Parent = _diskProvider.GetParent(path) };
        }
        catch (UnauthorizedAccessException)
        {
            return new FileSystemResult { Parent = _diskProvider.GetParent(path) };
        }

        return result;
    }

    #endregion
}