namespace PlexRipper.BaseTests;

public class PlexApiDataConfig
{
    public int Seed { get; set; }

    public int GetSeed()
    {
        return ++Seed;
    }

    public static PlexApiDataConfig FromOptions(
        Action<PlexApiDataConfig> action = null,
        PlexApiDataConfig defaultValue = null)
    {
        var config = defaultValue ?? new PlexApiDataConfig();
        action?.Invoke(config);
        return config;
    }
}