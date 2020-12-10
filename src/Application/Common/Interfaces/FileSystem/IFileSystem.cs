using System.IO;
using FluentResults;
using PlexRipper.Domain;

namespace PlexRipper.Application.Common
{
    public interface IFileSystem
    {
        string RootDirectory { get; }

        string ConfigDirectory { get; }

        Result<FileStream> SaveFile(string directory, string fileName, long fileSize);

        string ToAbsolutePath(string relativePath);

        FileSystemResult LookupContents(string query, bool includeFiles, bool allowFoldersWithoutTrailingSlashes);

        Result<FileStream> DownloadWorkerTempFileStream(string directory, string fileName, long fileSize);

        Result Setup();
    }
}