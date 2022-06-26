using FluentResults;
using PlexRipper.Application;
using PlexRipper.Domain;

namespace PlexRipper.BaseTests
{
    public class MockFileSystem : IFileSystem
    {
        public Result Setup()
        {
            return Result.Ok();
        }

        public Result<Stream> SaveFile(string directory, string fileName, long fileSize)
        {
            return Result.Ok();
        }

        public string ToAbsolutePath(string relativePath)
        {
            return "";
        }

        public Result<FileSystemResult> LookupContents(string query, bool includeFiles, bool allowFoldersWithoutTrailingSlashes)
        {
            return Result.Ok();
        }

        public Result CreateDirectoryFromFilePath(string filePath)
        {
            return Result.Ok();
        }

        public Result DeleteDirectoryFromFilePath(string filePath)
        {
            return Result.Ok();
        }

        public Result DeleteAllFilesFromDirectory(string directory)
        {
            return Result.Ok();
        }

        public bool FileExists(string path)
        {
            return true;
        }

        public Result<string> FileReadAllText(string path)
        {
            return Result.Ok();
        }

        public Result FileWriteAllText(string path, string text)
        {
            return Result.Ok();
        }

        public Result CreateDirectory(string directory)
        {
            return Result.Ok();
        }

        public Result<Stream> Open(string path, FileMode mode, FileAccess access, FileShare share)
        {
            return Result.Ok();
        }

        public Result<Stream> Create(string path, int bufferSize, FileOptions options)
        {
            return Result.Ok();
        }

        public Result FileMove(string sourceFileName, string destFileName, bool overwrite = true)
        {
            return Result.Ok();
        }

        public Result DeleteFile(string filePath)
        {
            return Result.Ok();
        }
    }
}