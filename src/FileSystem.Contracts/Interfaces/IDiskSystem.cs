using FluentResults;

namespace FileSystem.Contracts;

public interface IDiskSystem
{
    Result<long> GetAvailableSpaceByDirectory(string directory);
}
