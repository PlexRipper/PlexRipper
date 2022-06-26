namespace PlexRipper.Application;

public interface IDiskSystem
{
    Result<long> GetAvailableSpaceByDirectory(string directory);

    Result HasDirectoryEnoughAvailableSpace(string directory, long fileSize);
}