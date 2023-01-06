namespace PlexRipper.BaseTests;

public class PlexApiDataConfig
{
    public int Seed { get; set; }

    public int GetSeed()
    {
        return ++Seed;
    }

    public int LibraryCount { get; set; } = 3;

    public int LibraryMetaDataCount { get; set; } = 50;

    public static PlexApiDataConfig FromOptions(
        Action<PlexApiDataConfig> action = null,
        PlexApiDataConfig defaultValue = null)
    {
        var config = defaultValue ?? new PlexApiDataConfig();
        action?.Invoke(config);
        return config;
    }
}