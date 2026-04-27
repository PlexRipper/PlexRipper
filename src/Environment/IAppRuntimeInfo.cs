using Serilog.Events;

namespace Reaparr.Environment;

/// <summary>
/// Provides runtime storage paths resolved from Reaparr environment variables.
/// Values map to <see cref="EnvKeys"/> keys and are <see langword="null"/> when
/// the corresponding environment variable is not set or contains only whitespace.
/// </summary>
public interface IAppRuntimeInfo
{
    /// <summary>
    /// Gets the process user ID (PUID) from the environment. Returns -1 when not set or invalid.
    /// </summary>
    int PUID { get; }

    /// <summary>
    /// Gets the process group ID (PGID) from the environment. Returns -1 when not set or invalid.
    /// </summary>
    int PGID { get; }

    /// <summary>
    /// Gets the GitHub token from <c>GITHUB_TOKEN</c> for authenticated GitHub API requests.
    /// </summary>
    string? GitHubToken { get; }

    /// <summary>
    /// Gets all defined System and user level Environment keys and their values
    /// </summary>
    Dictionary<string, string?> GetAllEnvironmentVariables { get; }

    #region Paths

    /// <summary>
    /// Gets the base data path from <see cref="EnvKeys.ReaparrDataPath"/>.
    /// </summary>
    string? DataPath { get; }

    /// <summary>
    /// Gets the default configuration path from <see cref="EnvKeys.ReaparrConfigPath"/>.
    /// </summary>
    string? ConfigPath { get; }

    /// <summary>
    /// Gets the default downloads path from <see cref="EnvKeys.ReaparrDownloadsPath"/>.
    /// </summary>
    string? DownloadsPath { get; }

    /// <summary>
    /// Gets the default movies library path from <see cref="EnvKeys.ReaparrMoviesPath"/>.
    /// </summary>
    string? MoviesPath { get; }

    /// <summary>
    /// Gets the default TV shows library path from <see cref="EnvKeys.ReaparrTvShowsPath"/>.
    /// </summary>
    string? TvShowsPath { get; }

    /// <summary>
    /// Gets the default music library path from <see cref="EnvKeys.ReaparrMusicPath"/>.
    /// </summary>
    string? MusicPath { get; }

    /// <summary>
    /// Gets the default photos library path from <see cref="EnvKeys.ReaparrPhotosPath"/>.
    /// </summary>
    string? PhotosPath { get; }

    /// <summary>
    /// Gets the miscellaneous library path from <see cref="EnvKeys.ReaparrOtherPath"/>.
    /// </summary>
    string? OtherPath { get; }

    /// <summary>
    /// Gets the default games library path from <see cref="EnvKeys.ReaparrGamesPath"/>.
    /// </summary>
    string? GamesPath { get; }

    string? AppImage { get; }

    /// <summary>
    /// When set to true, the application will log all environment variables set on startup
    /// </summary>
    bool ShouldLogEnvVars { get; }

    /// <summary>
    /// Sets the SEQ_URL environment variable to the specified URL.
    /// Note: This is used for development and testing purposes to redirect logs to a hosted Docker instance of Seq.
    /// </summary>
    string SEQ_Url { get; }

    /// <summary>
    /// Returns true when <c>IntegrationTestMode</c> environment variable is set to a truthy value.
    /// </summary>
    bool IsIntegrationTestMode { get; }

    /// <summary>
    /// Returns true if the DOTNET_ENVIRONMENT is set to Development.
    /// </summary>
    bool IsDevelopmentEnvironment { get; }

    /// <summary>
    /// Returns true if the DOTNET_ENVIRONMENT is set to Production.
    /// </summary>
    bool IsProductionEnvironment { get; }

    /// <summary>
    /// Gets the name of the HTTP header used for bearer/auth token passing. Defaults to <c>X-Auth-User</c>.
    /// </summary>
    string HeaderAuthTokenName { get; }

    /// <summary>
    /// When set to true, the application will not mask/censor sensitive data in the logs.
    /// </summary>
    bool IsUnmasked { get; }

    /// <summary>
    /// Gets the configured Serilog log level from <c>LOG_LEVEL</c>. Defaults to <see cref="LogEventLevel.Debug"/>.
    /// </summary>
    LogEventLevel LogLevel { get; }

    /// <summary>
    /// When set to a truthy value, disables all authentication. FOR DEVELOPMENT USE ONLY.
    /// </summary>
    bool IsAuthenticationDisabled { get; }

    /// <summary>
    /// Gets the port number from the DOTNET_HTTP_PORTS environment variable.
    /// </summary>
    /// <returns>The port number or 5000 if not configured.</returns>
    int AppPort { get; }

    #endregion
}
