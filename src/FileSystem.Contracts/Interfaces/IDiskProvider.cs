using FluentResults;
using PlexRipper.Domain;

namespace FileSystem.Contracts;

public interface IDiskProvider
{
    string GetParent(string path);

    Result<List<FileSystemModel>> GetFiles(string path);

    Result<List<FileSystemModel>> GetDirectories(string path);

    string GetDirectoryPath(string path);

    Result<List<DirectoryInfo>> GetDirectoryInfos(string path);

    List<DriveInfo> GetAllMounts();

    string GetVolumeName(DriveInfo mountInfo);
}
