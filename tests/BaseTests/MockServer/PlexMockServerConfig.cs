namespace PlexRipper.BaseTests;

public class PlexMockServerConfig : BaseConfig<PlexMockServerConfig>
{
    public int DownloadFileSizeInMb { get; init; } = 0;

    /// <summary>
    ///  Gets or sets the fake data configuration for this mock server.
    /// </summary>
    public Action<PlexApiDataConfig>? FakeDataConfig { get; set; }

    public static string FileUrl => "/library/parts/653125/119385313456/file.mp4";
}
