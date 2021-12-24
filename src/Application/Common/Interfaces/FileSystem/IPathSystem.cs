using FluentResults;

namespace PlexRipper.Application
{
    public interface IPathSystem
    {
        Result<string> Combine(params string[] paths);

        string GetPathRoot(string directory);
    }
}