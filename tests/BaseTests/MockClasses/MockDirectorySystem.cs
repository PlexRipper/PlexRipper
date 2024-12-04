using System.IO.Abstractions;
using FileSystem.Contracts;

namespace PlexRipper.BaseTests;

public class MockDirectorySystem : IDirectorySystem
{
    public Result<bool> Exists(string path) => Result.Ok();

    public Result<IDirectoryInfo> CreateDirectory(string path) => Result.Ok();

    public Result CreateDirectoryFromFilePath(string filePath) => Result.Ok();

    public Result DeleteAllFilesFromDirectory(string directory) => Result.Ok();

    public Result<string[]> GetFiles(string directoryPath) => Result.Ok();

    public Result DirectoryDelete(string directoryPath) => Result.Ok();

    public Result DeleteDirectoryFromFilePath(string filePath) => Result.Ok();
}
