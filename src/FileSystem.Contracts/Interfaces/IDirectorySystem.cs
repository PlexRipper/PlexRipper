using FluentResults;

namespace FileSystem.Contracts;

public interface IDirectorySystem
{
    /// <summary>
    /// Check if the directory exists.
    /// </summary>
    /// <param name="path">The directory path to check if it exists.</param>
    /// <returns> <b>true</b> if path refers to an existing directory; <b>false</b> if the directory does not exist </returns>
    Result<bool> Exists(string path);

    Result<DirectoryInfo> CreateDirectory(string path);

    Result CreateDirectoryFromFilePath(string filePath);

    /// <summary>
    /// Deletes all files from the directory if it exists
    /// </summary>
    /// <param name="directory">The directory to delete all files from.</param>
    /// <returns></returns>
    Result DeleteAllFilesFromDirectory(string directory);

    Result<string[]> GetFiles(string directoryPath);

    Result DirectoryDelete(string directoryPath);

    /// <summary>
    /// Deletes the last folder in the file path.
    /// Note: if the filePath is an empty directory, then that last folder will be deleted, if empty.
    /// </summary>
    /// <param name="filePath">The filepath to delete the last folder in the path from.</param>
    /// <returns></returns>
    Result DeleteDirectoryFromFilePath(string filePath);
}
