namespace PlexRipper.Application;

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