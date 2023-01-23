using FluentResults;

namespace FileSystem.Contracts;

public interface IDiskSystem
{
    Result<long> GetAvailableSpaceByDirectory(string directory);

    Result HasDirectoryEnoughAvailableSpace(string directory, long fileSize);
}