using System.Reflection;

namespace Reaparr.Environment;

/// <inheritdoc/>
public class AppBuildInfo : IAppBuildInfo
{
    private readonly List<AssemblyMetadataAttribute> _attributes;

    /// <inheritdoc/>
    public string GetVersion { get; }

    /// <inheritdoc/>
    public string GetInformationalVersion { get; }

    /// <inheritdoc/>
    public string GetRuntimeMode => GetAssemblyMetadataValue("ReaparrRuntimeMode");

    /// <inheritdoc/>
    public string GetRuntimeIdentifier => GetAssemblyMetadataValue("ReaparrRuntimeIdentifier");

    /// <inheritdoc/>
    public bool IsDesktopMode => GetRuntimeMode.Contains("desktop", StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public bool IsDockerMode => GetRuntimeMode.Contains("docker", StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public bool IsDevRelease => GetInformationalVersion.Contains("dev", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Initializes a new instance of <see cref="AppBuildInfo"/> by reading build metadata from the entry assembly.
    /// </summary>
    public AppBuildInfo()
    {
        var assembly = Assembly.GetEntryAssembly();
        if (assembly is null)
            throw new ArgumentNullException(nameof(assembly));

        _attributes = assembly.GetCustomAttributes<AssemblyMetadataAttribute>().ToList();
        GetVersion = assembly.GetName().Version?.ToString() ?? "0.0.0";
        GetInformationalVersion =
            assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "0.0.0";
    }

    private string GetAssemblyMetadataValue(string key) =>
        _attributes
            .FirstOrDefault(attribute => string.Equals(attribute.Key, key, StringComparison.OrdinalIgnoreCase))
            ?.Value
        ?? string.Empty;
}
