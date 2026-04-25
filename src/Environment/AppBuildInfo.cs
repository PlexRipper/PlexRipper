using System.Reflection;

namespace Reaparr.Environment;

public class AppBuildInfo : IAppBuildInfo
{
    private readonly List<AssemblyMetadataAttribute> _attributes;

    public string? GetRuntimeMode => GetAssemblyMetadataValue("ReaparrRuntimeMode");

    public bool IsDesktopMode => GetRuntimeMode == "desktop";

    public bool IsDockerMode => GetRuntimeMode == "docker";

    public AppBuildInfo()
    {
        var assembly = Assembly.GetEntryAssembly();
        if (assembly is null)
            throw new ArgumentNullException(nameof(assembly));
        
        _attributes = assembly.GetCustomAttributes<AssemblyMetadataAttribute>().ToList();
    }

    private string? GetAssemblyMetadataValue(string key)
    {
        return _attributes.FirstOrDefault(attribute => string.Equals(attribute.Key, key, StringComparison.OrdinalIgnoreCase))?.Value;
    }
}
