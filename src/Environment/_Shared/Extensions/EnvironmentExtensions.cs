using Serilog.Events;

namespace Reaparr.Environment;

public static class EnvironmentExtensions
{
    #region Constants

    private const string INTEGRATION_TEST_MODE_KEY = "IntegrationTestMode";

    public const string UnmaskedModeKey = "UNMASKED";

    private const string LOG_ENV_VARS_KEY = "LOG_ENV_VARS";

    private const string LOG_LEVEL_KEY = "LOG_LEVEL";

    private const string VERSION_KEY = "VERSION";

    private const string INFORMATIONAL_VERSION_KEY = "INFORMATIONAL_VERSION";

    private const string REAPARR_PLATFORM_KEY = "REAPARR_PLATFORM";

    private const string REAPARR_DATA_PATH_KEY = "REAPARR_DATA_PATH";

    private const string REAPARR_CONFIG_PATH_KEY = "REAPARR_CONFIG_PATH";

    private const string AUTH_HEADER_TOKEN_NAME = "AUTH_HEADER_TOKEN";

    private const string SEQ_URL = "SEQ_URL";

    #endregion

    #region Getters

    /// <summary>
    /// Returns true when <c>IntegrationTestMode</c> environment variable is set to a truthy value.
    /// </summary>
    public static bool IsIntegrationTestMode() => IsTrue(GetEnvironmentVariable(INTEGRATION_TEST_MODE_KEY));

    public static string? GetDataPath() => GetEnvironmentVariable(REAPARR_DATA_PATH_KEY);

    public static string? GetConfigPath() => GetEnvironmentVariable(REAPARR_CONFIG_PATH_KEY);

    public static string GetReaparrMode() =>
        GetEnvironmentVariable(REAPARR_PLATFORM_KEY)?.ToLowerInvariant() == "desktop" ? "desktop" : "docker";

    public static bool IsDesktopMode() => GetReaparrMode() == "desktop";

    public static bool IsDockerMode() => GetReaparrMode() == "docker";

    /// <summary>
    /// Gets the name of the HTTP header used for bearer/auth token passing. Defaults to <c>X-Auth-User</c>.
    /// </summary>
    public static string GetHeaderAuthTokenName() => GetEnvironmentVariable(AUTH_HEADER_TOKEN_NAME) ?? "X-Auth-User";

    /// <summary>
    /// When set to true, the application will not mask/censor sensitive data in the logs.
    /// </summary>
    public static bool IsUnmasked() => IsTrue(GetEnvironmentVariable(UnmaskedModeKey));

    /// <summary>
    /// When set to true, the application will log all environment variables set on startup
    /// </summary>
    /// <returns></returns>
    public static bool ShouldLogEnvVars() => IsTrue(GetEnvironmentVariable(LOG_ENV_VARS_KEY));

    /// <summary>
    /// Gets the configured Serilog log level from <c>LOG_LEVEL</c>. Defaults to <see cref="LogEventLevel.Debug"/>.
    /// </summary>
    public static LogEventLevel GetLogLevel()
    {
        var success = Enum.TryParse<LogEventLevel>(GetEnvironmentVariable(LOG_LEVEL_KEY), true, out var logLevel);
        return success ? logLevel : LogEventLevel.Debug;
    }

    /// <summary>
    /// Gets the application version from <c>INFORMATIONAL_VERSION</c> or <c>VERSION</c>. Defaults to <c>0.0.0</c>.
    /// </summary>
    public static string GetVersion() =>
        GetEnvironmentVariable(INFORMATIONAL_VERSION_KEY) ?? GetEnvironmentVariable(VERSION_KEY) ?? "0.0.0";

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
    public static string GetSeqUrl() => GetEnvironmentVariable(SEQ_URL) ?? "http://localhost:5341";

    #endregion

    #region Setters

    public static void SetPort(int port) =>
        System.Environment.SetEnvironmentVariable("DOTNET_HTTP_PORTS", port.ToString());

    /// <summary>
    /// Sets the <c>LOG_LEVEL</c> environment variable to the specified level (upper-cased).
    /// </summary>
    public static void SetLogLevel(LogEventLevel logLevel)
    {
        System.Environment.SetEnvironmentVariable(LOG_LEVEL_KEY, logLevel.ToString().ToUpper());
    }

    public static void SetDataPath(string path) =>
        System.Environment.SetEnvironmentVariable(REAPARR_DATA_PATH_KEY, path);

    public static void SetConfigPath(string path) =>
        System.Environment.SetEnvironmentVariable(REAPARR_CONFIG_PATH_KEY, path);

    /// <summary>
    /// Enables or disables integration test mode by setting <c>IntegrationTestMode</c>.
    /// </summary>
    public static void SetIntegrationTestMode(bool state)
    {
        System.Environment.SetEnvironmentVariable(INTEGRATION_TEST_MODE_KEY, state.ToString());
    }

    /// <summary>
    /// When set to true, the application will not mask/censor sensitive data in the logs.
    /// </summary>
    public static void EnableUnmaskedLog(bool state)
    {
        System.Environment.SetEnvironmentVariable(UnmaskedModeKey, state.ToString());
    }

    /// <summary>
    /// When set to true, the application will log all environment variables set on startup.
    /// </summary>
    public static void EnableLogEnvVars(bool state)
    {
        System.Environment.SetEnvironmentVariable(LOG_ENV_VARS_KEY, state.ToString());
    }

    #endregion

    #region Helpers

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
        value is not null && (value == Convert.ToString(true) || value == "1" || value == "true" || value == "TRUE");

    #endregion
}
