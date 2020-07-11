using FluentResults;
using PlexRipper.Domain.Types.FileSystem;
using System.IO;

namespace PlexRipper.Application.Common.Interfaces.FileSystem
{
    public interface IFileSystem
    {
        string RootDirectory { get; }
        string ConfigDirectory { get; }
        Result<FileStream> SaveFile(string directory, string fileName, long fileSize);
        string ToAbsolutePath(string relativePath);
        FileSystemResult LookupContents(string query, bool includeFiles, bool allowFoldersWithoutTrailingSlashes);
    }
}
