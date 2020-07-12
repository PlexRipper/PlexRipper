using PlexRipper.Domain.Types.FileSystem;
using System.Collections.Generic;
using System.IO;

namespace PlexRipper.Application.Common.Interfaces.FileSystem
{
    public interface IDiskProvider
    {
        FileSystemResult GetResult(string path, bool includeFiles);
        string GetParent(string path);
        List<FileSystemModel> GetFiles(string path);
        List<FileSystemModel> GetDirectories(string path);
        string GetDirectoryPath(string path);
        List<DirectoryInfo> GetDirectoryInfos(string path);
        List<IMount> GetMounts();
        string GetVolumeName(IMount mountInfo);
    }
}
