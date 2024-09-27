namespace PlexRipper.BaseTests;

public class PlexMockServerConfig : BaseConfig<PlexMockServerConfig>
{
    public int DownloadFileSizeInMb { get; init; } = 0;
    public Action<PlexApiDataConfig> FakeDataConfig { get; init; }

    public static string FileUrl => "/library/parts/653125/119385313456/file.mp4";
}
