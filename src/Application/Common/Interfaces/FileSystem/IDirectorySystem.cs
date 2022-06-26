namespace PlexRipper.Application
{
    public interface IDirectorySystem
    {
        Result<bool> Exists(string path);

        Result<DirectoryInfo> CreateDirectory(string path);

        Result CreateDirectoryFromFilePath(string filePath);

        /// <inheritdoc />
        Result DeleteAllFilesFromDirectory(string directory);

        Result<string[]> GetFiles(string directoryPath);

        Result Delete(string directoryPath);

        /// <summary>
        /// Deletes the last folder in the file path.
        /// Note: if the filePath is an empty directory, then that last folder will be deleted, if empty.
        /// </summary>
        /// <param name="filePath">The filepath to delete the last folder in the path from.</param>
        /// <returns></returns>
        Result DeleteDirectoryFromFilePath(string filePath);
    }
}