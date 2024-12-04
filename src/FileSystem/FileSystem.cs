using System.IO.Abstractions;
using FileSystem.Contracts;
using Logging.Interface;

namespace PlexRipper.FileSystem;

/// <inheritdoc/>
public class FileResultSystem : IFileResultSystem
{
    #region Fields

    private readonly ILog _log;

    private readonly IFileSystem _abstractedFileSystem;

    private readonly IDiskProvider _diskProvider;
    private readonly IDirectory _directory;

    #endregion

    #region Constructor

    public FileResultSystem(
        ILog log,
        IFileSystem abstractedFileSystem,
        IDiskProvider diskProvider,
        IDirectory directory
    )
    {
        _log = log;
        _abstractedFileSystem = abstractedFileSystem;
        _diskProvider = diskProvider;
        _directory = directory;
    }

    #endregion

    #region Public Methods

    public bool FileExists(string path) => !string.IsNullOrEmpty(path) && _abstractedFileSystem.File.Exists(path);

    public Result<Stream> Open(string path, FileMode mode, FileAccess access, FileShare share)
    {
        try
        {
            Stream openStream = _abstractedFileSystem.File.Open(path, mode, access, share);
            return Result.Ok(openStream);
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
            Stream createStream = _abstractedFileSystem.File.Create(path, bufferSize, options);
            return Result.Ok(createStream);
        }
        catch (Exception e)
        {
            return Result.Fail(new ExceptionalError(e)).LogError();
        }
    }

    public Result Copy(string sourceFileName, string destFileName)
    {
        try
        {
            _abstractedFileSystem.File.Copy(sourceFileName, destFileName);
            return Result.Ok();
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

    #endregion
}
