namespace PlexRipper.BaseTests;

public class PlexApiDataConfig : BaseConfig<PlexApiDataConfig>
{
    public Seed Seed { get; set; } = new(9999);

    public int LibraryCount { get; set; } = 3;

    public int LibraryMetaDataCount { get; set; } = 50;

    public int PlexServerAccessCount { get; set; } = 5;

    public int PlexServerAccessConnectionsCount { get; set; } = 5;

    public bool PlexServerAccessConnectionsIncludeHttps { get; set; } = false;
}
