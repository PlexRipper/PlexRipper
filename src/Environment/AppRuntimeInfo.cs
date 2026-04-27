using System.Collections;

namespace Reaparr.Environment;

public class AppRuntimeInfo : IAppRuntimeInfo
{
    /// <inheritdoc/>
    public bool IsDevelopmentEnvironment => GetEnvironmentVariable(EnvKeys.DotNetEnvironment) == "Development";

    /// <inheritdoc/>
    public bool IsProductionEnvironment => GetEnvironmentVariable(EnvKeys.DotNetEnvironment) == "Production";

    /// <inheritdoc/>
    public bool IsIntegrationTestMode => IsTrue(GetEnvironmentVariable(EnvKeys.IntegrationTestMode));

    /// <inheritdoc/>
    public string SEQ_Url => GetEnvironmentVariable(EnvKeys.SeqUrl) ?? "http://localhost:5341";

    /// <inheritdoc/>
    public int PUID => int.TryParse(GetEnvironmentVariable(EnvKeys.Puid), out var puid) ? puid : -1;

    /// <inheritdoc/>
    public int PGID => int.TryParse(GetEnvironmentVariable(EnvKeys.Pgid), out var pgid) ? pgid : -1;

    /// <inheritdoc/>
    public string? GitHubToken => GetEnvironmentVariable(EnvKeys.GitHubToken);

    /// <inheritdoc/>
    public string? AppImage => GetEnvironmentVariable(EnvKeys.AppImage);

    /// <inheritdoc/>
    public bool ShouldLogEnvVars => IsTrue(GetEnvironmentVariable(EnvKeys.LogEnvironmentVariables));

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

    public Dictionary<string, string?> GetAllEnvironmentVariables => System.Environment
        .GetEnvironmentVariables()
        .Cast<DictionaryEntry>()
        .ToDictionary(
            entry => entry.Key.ToString()!,
            entry => entry.Value?.ToString()
        );

    /// <summary>
    /// Determines if the value is true.
    /// </summary>
    /// <param name="value"></param>
    private static bool IsTrue(string? value) => value is not null
                                                 && (string.Equals(value, Convert.ToString(true),
                                                     StringComparison.OrdinalIgnoreCase) || value == "1");
}