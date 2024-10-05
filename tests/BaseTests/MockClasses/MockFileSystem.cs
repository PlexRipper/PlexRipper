using FileSystem.Contracts;

namespace PlexRipper.BaseTests;

public class MockFileSystem : IFileSystem
{
    public Result Setup() => Result.Ok();

    public Result<Stream> SaveFile(string directory, string fileName, long fileSize) => Result.Ok();

    public string ToAbsolutePath(string relativePath) => "";

    public Result<FileSystemResult> LookupContents(
        string query,
        bool includeFiles,
        bool allowFoldersWithoutTrailingSlashes
    ) => Result.Ok();

    public Result CreateDirectoryFromFilePath(string filePath) => Result.Ok();

    public Result DeleteDirectoryFromFilePath(string filePath) => Result.Ok();

    public Result DeleteAllFilesFromDirectory(string directory) => Result.Ok();

    public bool FileExists(string path) => true;

    public Result<string> FileReadAllText(string path) => Result.Ok();

    public Result FileWriteAllText(string path, string text) => Result.Ok();

    public Result CreateDirectory(string directory) => Result.Ok();

    public Result<Stream> Open(string path, FileMode mode, FileAccess access, FileShare share) => Result.Ok();

    public Result<Stream> Create(string path, int bufferSize, FileOptions options) => Result.Ok();

    public Result FileMove(string sourceFileName, string destFileName, bool overwrite = true) => Result.Ok();

    public Result DeleteFile(string filePath) => Result.Ok();

    public Result Copy(string sourceFileName, string destFileName) => Result.Ok();
}
