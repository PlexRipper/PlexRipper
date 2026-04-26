namespace Reaparr.BaseTests;

public class MockAppBuildInfo : IAppBuildInfo
{
    /// <inheritdoc/>
    public string RuntimeMode { get; set; } = "docker";

    /// <inheritdoc/>
    public string RuntimeIdentifier { get; init; } = "linux-x64";

    /// <inheritdoc/>
    public string Version { get; set; } = "0.0.0";

    /// <inheritdoc/>
    public string InformationalVersion { get; set; } = "0.0.0";

    /// <inheritdoc/>
    public bool IsDesktopMode => RuntimeMode.Contains("desktop", StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public bool IsDockerMode => RuntimeMode.Contains("docker", StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public bool IsDevRelease => InformationalVersion.Contains("dev", StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public OperatingSystemPlatform CurrentOS { get; init; } = OperatingSystemPlatform.Linux;

    /// <inheritdoc/>
    public bool IsWindows => CurrentOS == OperatingSystemPlatform.Windows;
}
