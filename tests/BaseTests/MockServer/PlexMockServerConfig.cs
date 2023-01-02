namespace PlexRipper.BaseTests;

public class PlexMockServerConfig
{
    public int DownloadFileSizeInMb { get; set; } = 0;

    public static string FileUrl => "/library/parts/65125/1193813456/file.mp4";

    public static PlexMockServerConfig FromOptions(Action<PlexMockServerConfig> action = null, PlexMockServerConfig defaultValue = null)
    {
        var config = defaultValue ?? new PlexMockServerConfig();
        action?.Invoke(config);
        return config;
    }

    public static List<PlexMockServerConfig> FromOptions(Action<List<PlexMockServerConfig>> action = null, List<PlexMockServerConfig> defaultValue = null)
    {
        var config = defaultValue ?? new List<PlexMockServerConfig>();
        action?.Invoke(config);
        return config;
    }
}