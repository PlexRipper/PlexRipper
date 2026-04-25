namespace Reaparr.Environment;

/// <summary>
/// Provides build-time and assembly-stamped metadata for the running Reaparr application.
/// </summary>
public interface IAppBuildInfo
{
    /// <summary>
    /// Gets the application runtime mode stamped into the build metadata, for example <c>desktop</c> or <c>docker</c>.
    /// </summary>
    string? GetRuntimeMode { get; }

    /// <summary>
    /// Gets the runtime identifier stamped into the build metadata, for example <c>linux-arm64</c>.
    /// </summary>
    string GetRuntimeIdentifier { get; }

    /// <summary>
    /// Gets the application release version from <c>VERSION</c>.
    /// This should be the stable product version, for example <c>0.36.1</c>.
    /// Falls back to <c>0.0.0</c> when the environment variable is not set.
    /// </summary>
    string GetVersion { get; }

    /// <summary>
    /// Gets the application informational version from <c>INFORMATIONAL_VERSION</c>.
    /// This is typically a more detailed build string than <c>VERSION</c>, for example <c>0.36.0-dev.1</c>.
    /// Falls back to <c>0.0.0</c> when the environment variable is not set.
    /// </summary>
    string GetInformationalVersion { get; }

    /// <summary>
    /// Gets the application running mode and checks if it is Desktop
    /// </summary>
    bool IsDesktopMode { get; }

    /// <summary>
    /// Gets the application running mode and checks if it is Docker
    /// </summary>
    bool IsDockerMode { get; }

    /// <summary>
    /// Returns true if the current version indicates a development build (contains <c>dev</c>).
    /// </summary>
    bool IsDevRelease { get; }

    /// <summary>
    /// Gets the current operating system platform as an <see cref="OperatingSystemPlatform"/> enum value.
    /// </summary>
    OperatingSystemPlatform CurrentOS { get; }

    /// <summary>
    /// Returns true if the current operating system is Windows.
    /// </summary>
    bool IsWindows { get; }
}