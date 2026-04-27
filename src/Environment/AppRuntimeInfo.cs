namespace Reaparr.Environment;

public class AppRuntimeInfo : IAppRuntimeInfo
{
    /// <inheritdoc/>
    public string? GitHubToken => GetEnvironmentVariable(EnvKeys.GitHubToken);

    #region Paths

    /// <inheritdoc/>
    public string? DataPath => GetEnvironmentVariable(EnvKeys.ReaparrDataPath);

    /// <inheritdoc/>
    public string? ConfigPath => GetEnvironmentVariable(EnvKeys.ReaparrConfigPath);

    /// <inheritdoc/>
    public string? DownloadsPath => GetEnvironmentVariable(EnvKeys.ReaparrDownloadsPath);

    /// <inheritdoc/>
    public string? MoviesPath => GetEnvironmentVariable(EnvKeys.ReaparrMoviesPath);

    /// <inheritdoc/>
    public string? TvShowsPath => GetEnvironmentVariable(EnvKeys.ReaparrTvShowsPath);

    /// <inheritdoc/>
    public string? MusicPath => GetEnvironmentVariable(EnvKeys.ReaparrMusicPath);

    /// <inheritdoc/>
    public string? PhotosPath => GetEnvironmentVariable(EnvKeys.ReaparrPhotosPath);

    /// <inheritdoc/>
    public string? OtherPath => GetEnvironmentVariable(EnvKeys.ReaparrOtherPath);

    /// <inheritdoc/>
    public string? GamesPath => GetEnvironmentVariable(EnvKeys.ReaparrGamesPath);

    #endregion

    public static string? GetEnvironmentVariable(string key)
    {
        var value = System.Environment.GetEnvironmentVariable(key)?.Trim();
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }
}