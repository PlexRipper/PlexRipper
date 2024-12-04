using FluentResults;
using PlexRipper.Domain;

namespace FileSystem.Contracts;

/// <summary>
/// This is a wrapper class for the System.IO.Abstractions.FileSystem class to wrap all file system operations in Result objects.
/// </summary>
public interface IFileResultSystem
{
    Result<FileSystemResult> LookupContents(string query, bool includeFiles, bool allowFoldersWithoutTrailingSlashes);

    bool FileExists(string path);

    Result<string> FileReadAllText(string path);

    Result FileWriteAllText(string path, string text);

    Result<Stream> Open(string path, FileMode mode, FileAccess access, FileShare share);

    Result<Stream> Create(string path, int bufferSize, FileOptions options);

    Result FileMove(string sourceFileName, string destFileName, bool overwrite = true);

    Result DeleteFile(string filePath);

    Result Copy(string sourceFileName, string destFileName);
}
