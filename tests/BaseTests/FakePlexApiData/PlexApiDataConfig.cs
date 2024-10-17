namespace PlexRipper.BaseTests;

public class PlexApiDataConfig : BaseConfig<PlexApiDataConfig>
{
    public int LibraryCount { get; set; } = 3;

    public int LibraryMetaDataCount { get; set; } = 50;

    public int PlexServerAccessCount { get; set; } = 5;

    public int PlexServerAccessConnectionsCount { get; set; } = 5;

    public bool PlexServerAccessConnectionsIncludeHttps { get; set; } = false;
}
