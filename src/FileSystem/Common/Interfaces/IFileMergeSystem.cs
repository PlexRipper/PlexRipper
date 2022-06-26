namespace PlexRipper.FileSystem.Common
{
    public interface IFileMergeSystem
    {
        bool FileExists(string path);

        Result DeleteDirectoryFromFilePath(string path);

        Result DeleteAllFilesFromDirectory(string directory);
    }
}