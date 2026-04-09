using Serilog.Events;

namespace Reaparr.Environment;

public static class EnvironmentExtensions
{
    #region Getters

    /// <summary>
    /// Returns true when <c>IntegrationTestMode</c> environment variable is set to a truthy value.
    /// </summary>
    public static bool IsIntegrationTestMode() =>
        IsTrue(GetEnvironmentVariable(EnvironmentVariableKey.IntegrationTestMode));

    public static string? GetDataPath() => GetEnvironmentVariable(EnvironmentVariableKey.ReaparrDataPath);

    public static string? GetConfigPath() => GetEnvironmentVariable(EnvironmentVariableKey.ReaparrConfigPath);

    public static string? GetDownloadsPath() => GetEnvironmentVariable(EnvironmentVariableKey.ReaparrDownloadsPath);

    public static string? GetMoviesPath() => GetEnvironmentVariable(EnvironmentVariableKey.ReaparrMoviesPath);

    public static string? GetTvShowsPath() => GetEnvironmentVariable(EnvironmentVariableKey.ReaparrTvShowsPath);

    public static string? GetMusicPath() => GetEnvironmentVariable(EnvironmentVariableKey.ReaparrMusicPath);

    public static string? GetPhotosPath() => GetEnvironmentVariable(EnvironmentVariableKey.ReaparrPhotosPath);

    public static string? GetOtherPath() => GetEnvironmentVariable(EnvironmentVariableKey.ReaparrOtherPath);

    public static string? GetGamesPath() => GetEnvironmentVariable(EnvironmentVariableKey.ReaparrGamesPath);

    public static string GetReaparrMode() =>
        GetEnvironmentVariable(EnvironmentVariableKey.ReaparrPlatform)?.ToLowerInvariant() == "desktop"
            ? "desktop"
            : "docker";

    public static bool IsDesktopMode() => GetReaparrMode() == "desktop";

    public static bool IsDockerMode() => GetReaparrMode() == "docker";

    /// <summary>
    /// Gets the name of the HTTP header used for bearer/auth token passing. Defaults to <c>X-Auth-User</c>.
    /// </summary>
    public static string GetHeaderAuthTokenName() =>
        GetEnvironmentVariable(EnvironmentVariableKey.AuthHeaderTokenName) ?? "X-Auth-User";

    /// <summary>
    /// When set to true, the application will not mask/censor sensitive data in the logs.
    /// </summary>
    public static bool IsUnmasked() => IsTrue(GetEnvironmentVariable(EnvironmentVariableKey.Unmasked));

    /// <summary>
    /// When set to true, the application will log all environment variables set on startup
    /// </summary>
    public static bool ShouldLogEnvVars() =>
        IsTrue(GetEnvironmentVariable(EnvironmentVariableKey.LogEnvironmentVariables));

    /// <summary>
    /// Gets the configured Serilog log level from <c>LOG_LEVEL</c>. Defaults to <see cref="LogEventLevel.Debug"/>.
    /// </summary>
    public static LogEventLevel GetLogLevel()
    {
        var success = Enum.TryParse<LogEventLevel>(
            GetEnvironmentVariable(EnvironmentVariableKey.LogLevel),
            true,
            out var logLevel
        );
        return success ? logLevel : LogEventLevel.Debug;
    }

    /// <summary>
    /// Gets the application version from <c>INFORMATIONAL_VERSION</c> or <c>VERSION</c>. Defaults to <c>0.0.0</c>.
    /// </summary>
    public static string GetVersion() =>
        GetEnvironmentVariable(EnvironmentVariableKey.InformationalVersion)
        ?? GetEnvironmentVariable(EnvironmentVariableKey.Version)
        ?? "0.0.0";

    /// <summary>
    /// Returns true if the current version indicates a development build (contains <c>dev</c>).
    /// </summary>
    public static bool IsDevRelease() => GetVersion().Contains("dev");

    /// <summary>
    /// Returns true if the DOTNET_ENVIRONMENT is set to Development.
    /// </summary>
    /// <returns></returns>
    public static bool IsDevelopmentEnvironment() => GetEnvironmentVariable("DOTNET_ENVIRONMENT") == "Development";

    /// <summary>
    /// Gets the process user ID (PUID) from the environment. Returns -1 when not set or invalid.
    /// </summary>
    public static int GetPuid() => int.TryParse(GetEnvironmentVariable("PUID"), out var puid) ? puid : -1;

    /// <summary>
    /// Gets the process group ID (PGID) from the environment. Returns -1 when not set or invalid.
    /// </summary>
    public static int GetPgid() => int.TryParse(GetEnvironmentVariable("PGID"), out var pgid) ? pgid : -1;

    /// <summary>
    /// Gets the port number from the DOTNET_HTTP_PORTS environment variable.
    /// </summary>
    /// <returns>The port number or 5000 if not configured.</returns>
    public static int GetPort =>
        int.TryParse(
            GetEnvironmentVariable("DOTNET_HTTP_PORTS")
                ?.Split(';', StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault(),
            out var port
        )
            ? port
            : 5000;

    /// <summary>
    /// Sets the SEQ_URL environment variable to the specified URL.
    /// Note: This is used for development and testing purposes to redirect logs to a hosted Docker instance of Seq.
    /// </summary>
    public static string GetSeqUrl() =>
        GetEnvironmentVariable(EnvironmentVariableKey.SeqUrl) ?? "http://localhost:5341";

    #endregion

    #region Setters

    public static void SetPort(int port) =>
        System.Environment.SetEnvironmentVariable("DOTNET_HTTP_PORTS", port.ToString());

    /// <summary>
    /// Sets the <c>LOG_LEVEL</c> environment variable to the specified level (upper-cased).
    /// </summary>
    public static void SetLogLevel(LogEventLevel logLevel)
    {
        System.Environment.SetEnvironmentVariable(
            EnvironmentVariableKey.LogLevel.Value(),
            logLevel.ToString().ToUpper()
        );
    }

    public static void SetDataPath(string path) =>
        System.Environment.SetEnvironmentVariable(EnvironmentVariableKey.ReaparrDataPath.Value(), path);

    public static void SetConfigPath(string path) =>
        System.Environment.SetEnvironmentVariable(EnvironmentVariableKey.ReaparrConfigPath.Value(), path);

    public static void SetDownloadsPath(string path) =>
        System.Environment.SetEnvironmentVariable(EnvironmentVariableKey.ReaparrDownloadsPath.Value(), path);

    public static void SetMoviesPath(string path) =>
        System.Environment.SetEnvironmentVariable(EnvironmentVariableKey.ReaparrMoviesPath.Value(), path);

    public static void SetTvShowsPath(string path) =>
        System.Environment.SetEnvironmentVariable(EnvironmentVariableKey.ReaparrTvShowsPath.Value(), path);

    public static void SetMusicPath(string path) =>
        System.Environment.SetEnvironmentVariable(EnvironmentVariableKey.ReaparrMusicPath.Value(), path);

    public static void SetPhotosPath(string path) =>
        System.Environment.SetEnvironmentVariable(EnvironmentVariableKey.ReaparrPhotosPath.Value(), path);

    public static void SetOtherPath(string path) =>
        System.Environment.SetEnvironmentVariable(EnvironmentVariableKey.ReaparrOtherPath.Value(), path);

    public static void SetGamesPath(string path) =>
        System.Environment.SetEnvironmentVariable(EnvironmentVariableKey.ReaparrGamesPath.Value(), path);

    /// <summary>
    /// Enables or disables integration test mode by setting <c>IntegrationTestMode</c>.
    /// </summary>
    public static void SetIntegrationTestMode(bool state)
    {
        System.Environment.SetEnvironmentVariable(EnvironmentVariableKey.IntegrationTestMode.Value(), state.ToString());
    }

    /// <summary>
    /// When set to true, the application will not mask/censor sensitive data in the logs.
    /// </summary>
    public static void EnableUnmaskedLog(bool state)
    {
        System.Environment.SetEnvironmentVariable(EnvironmentVariableKey.Unmasked.Value(), state.ToString());
    }

    /// <summary>
    /// When set to true, the application will log all environment variables set on startup.
    /// </summary>
    public static void EnableLogEnvVars(bool state)
    {
        System.Environment.SetEnvironmentVariable(
            EnvironmentVariableKey.LogEnvironmentVariables.Value(),
            state.ToString()
        );
    }

    #endregion

    #region Helpers

    public static string EnvironmentVariableName(EnvironmentVariableKey key) => key.Value();

    private static string? GetEnvironmentVariable(EnvironmentVariableKey key) => GetEnvironmentVariable(key.Value());

    private static string? GetEnvironmentVariable(string key)
    {
        var value = System.Environment.GetEnvironmentVariable(key)?.Trim();
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    /// <summary>
    /// Determines if the value is true.
    /// </summary>
    /// <param name="value"></param>
    private static bool IsTrue(string? value) =>
        value is not null
        && (string.Equals(value, Convert.ToString(true), StringComparison.OrdinalIgnoreCase) || value == "1");

    #endregion
}
