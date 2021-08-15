using System.IO;
using FluentResults;
using PlexRipper.Domain;

namespace PlexRipper.Application.Common
{
    public interface IFileSystem : ISetup
    {
        Result<Stream> SaveFile(string directory, string fileName, long fileSize);

        string ToAbsolutePath(string relativePath);

        Result<FileSystemResult> LookupContents(string query, bool includeFiles, bool allowFoldersWithoutTrailingSlashes);

        Result<Stream> DownloadWorkerTempFileStream(string directory, string fileName, long fileSize);

        Result CreateDirectoryFromFilePath(string filePath);

        /// <summary>
        /// Deletes the last folder in the file path.
        /// Note: if the filePath is an empty directory, then that last folder will be deleted, if empty.
        /// </summary>
        /// <param name="filePath">The filepath to delete the last folder in the path from.</param>
        /// <returns></returns>
        Result DeleteDirectoryFromFilePath(string filePath);

        /// <summary>
        /// Deletes all files recursively in a directory
        /// </summary>
        /// <param name="directory">The directory to delete all files from.</param>
        /// <returns></returns>
        Result DeleteAllFilesFromDirectory(string directory);

        Result<bool> FileExists(string path);

        Result<string> FileReadAllText(string path);

        Result FileWriteAllText(string path, string text);
    }
}