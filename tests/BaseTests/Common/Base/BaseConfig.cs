namespace PlexRipper.BaseTests;

public class BaseConfig
{
    public int Seed { get; set; }
}

public class BaseConfig<T> : BaseConfig where T : BaseConfig, new()
{
    public static T FromOptions(Action<T> action = null, T defaultValue = default)
    {
        var config = defaultValue ?? new T();
        action?.Invoke(config);
        return config;
    }
}