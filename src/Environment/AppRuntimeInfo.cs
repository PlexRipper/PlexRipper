namespace Reaparr.Environment;

public class AppRuntimeInfo : IAppRuntimeInfo
{
    public string? DataPath => GetEnvironmentVariable(EnvKeys.ReaparrDataPath);

    public string? ConfigPath => GetEnvironmentVariable(EnvKeys.ReaparrConfigPath);

    public string? DownloadsPath => GetEnvironmentVariable(EnvKeys.ReaparrDownloadsPath);

    public string? MoviesPath => GetEnvironmentVariable(EnvKeys.ReaparrMoviesPath);

    public string? TvShowsPath => GetEnvironmentVariable(EnvKeys.ReaparrTvShowsPath);

    public string? MusicPath => GetEnvironmentVariable(EnvKeys.ReaparrMusicPath);

    public string? PhotosPath => GetEnvironmentVariable(EnvKeys.ReaparrPhotosPath);

    public string? OtherPath => GetEnvironmentVariable(EnvKeys.ReaparrOtherPath);

    public string? GamesPath => GetEnvironmentVariable(EnvKeys.ReaparrGamesPath);

    public static string? GetEnvironmentVariable(string key)
    {
        var value = System.Environment.GetEnvironmentVariable(key)?.Trim();
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }
}
