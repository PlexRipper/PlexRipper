using FluentResults;
using PlexRipper.Domain;

namespace PlexRipper.Application
{
    public interface IFileSystem
    {
        string ToAbsolutePath(string relativePath);

        Result<FileSystemResult> LookupContents(string query, bool includeFiles, bool allowFoldersWithoutTrailingSlashes);

        bool FileExists(string path);

        Result<string> FileReadAllText(string path);

        Result FileWriteAllText(string path, string text);

        Result<Stream> Open(string path, FileMode mode, FileAccess access, FileShare share);

        Result<Stream> Create(string path, int bufferSize, FileOptions options);

        Result FileMove(string sourceFileName, string destFileName, bool overwrite = true);

        Result DeleteFile(string filePath);
    }
}