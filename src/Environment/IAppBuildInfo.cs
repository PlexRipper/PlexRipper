namespace Reaparr.Environment;

public interface IAppBuildInfo
{
    string? GetRuntimeMode { get; }

    bool IsDesktopMode { get; }

    bool IsDockerMode { get; }
}
