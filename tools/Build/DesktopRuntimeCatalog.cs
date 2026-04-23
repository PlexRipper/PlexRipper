namespace Reaparr.Build;

internal static class DesktopRuntimeCatalog
{
    private static readonly IReadOnlyDictionary<string, DesktopRuntime> Runtimes = new Dictionary<
        string,
        DesktopRuntime
    >(StringComparer.OrdinalIgnoreCase)
    {
        ["linux-x64"] = new("linux-x64", "Desktop-linux-x64", "Reaparr"),
        ["linux-arm64"] = new("linux-arm64", "Desktop-linux-arm64", "Reaparr"),
        ["win-x64"] = new("win-x64", "Desktop-win-x64", "Reaparr.exe"),
        ["win-arm64"] = new("win-arm64", "Desktop-win-arm64", "Reaparr.exe"),
        ["osx-x64"] = new("osx-x64", "Desktop-osx-x64", "Reaparr"),
        ["osx-arm64"] = new("osx-arm64", "Desktop-osx-arm64", "Reaparr"),
    };

    public static string SupportedRuntimeIdentifiers => string.Join(", ", Runtimes.Keys.Order());

    public static DesktopRuntime Get(string runtimeIdentifier)
    {
        if (Runtimes.TryGetValue(runtimeIdentifier, out var runtime))
        {
            return runtime;
        }

        throw new ArgumentOutOfRangeException(
            nameof(runtimeIdentifier),
            $"Unsupported desktop RID '{runtimeIdentifier}'. Supported values: {SupportedRuntimeIdentifiers}."
        );
    }
}
