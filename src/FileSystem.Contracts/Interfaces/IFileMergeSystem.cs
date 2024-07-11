using FluentResults;

namespace FileSystem.Contracts;

public interface IFileMergeSystem
{
    bool FileExists(string path);

    Result DeleteDirectoryFromFilePath(string path);

    Result DeleteAllFilesFromDirectory(string directory);
}
