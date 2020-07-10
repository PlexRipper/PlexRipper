using FluentResults;
using System.IO;

namespace PlexRipper.Application.Common.Interfaces.FileSystem
{
    public interface IFileSystem
    {
        string RootDirectory { get; }
        string ConfigDirectory { get; }
        Result<FileStream> SaveFile(string directory, string fileName, long fileSize);
        string ToAbsolutePath(string relativePath);
    }
}
