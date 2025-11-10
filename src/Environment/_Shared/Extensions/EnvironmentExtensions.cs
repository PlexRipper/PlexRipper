using Serilog.Events;

namespace Reaparr.Environment;

public static class EnvironmentExtensions
{
    private const string INTEGRATION_TEST_MODE_KEY = "IntegrationTestMode";

    public const string UnmaskedModeKey = "UNMASKED";

    private const string LOG_ENV_VARS_KEY = "LOG_ENV_VARS";

    private const string LOG_LEVEL_KEY = "LOG_LEVEL";

    private const string VERSION_KEY = "VERSION";

    private const string INFORMATIONAL_VERSION_KEY = "INFORMATIONAL_VERSION";

    private const string DEVELOPMENT_ROOT_PATH_KEY = "DEVELOPMENT_ROOT_PATH";

    private const string AUTH_HEADER_TOKEN_NAME = "AUTH_HEADER_TOKEN";

    private static readonly string _trueValue = Convert.ToString(true);

    /// <summary>
    /// Determines if the value is true.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private static bool IsTrue(string? value) =>
        value == _trueValue || value == "1" || value == "true" || value == "TRUE";

    /// <summary>
    /// Returns true when <c>IntegrationTestMode</c> environment variable is set to a truthy value.
    /// </summary>
    public static bool IsIntegrationTestMode() =>
        System.Environment.GetEnvironmentVariable(INTEGRATION_TEST_MODE_KEY) == _trueValue;

    /// <summary>
    /// This is the path that is used to store the /config, /downloads, /movies and /tvshows folders required to boot Reaparr in development mode in a non-docker environment.
    /// </summary>
    /// <returns></returns>
    public static string? GetDevelopmentRootPath() =>
        System.Environment.GetEnvironmentVariable(DEVELOPMENT_ROOT_PATH_KEY);

    /// <summary>
    /// Gets the name of the HTTP header used for bearer/auth token passing. Defaults to <c>X-Auth-User</c>.
    /// </summary>
    public static string GetHeaderAuthTokenName() =>
        System.Environment.GetEnvironmentVariable(AUTH_HEADER_TOKEN_NAME) ?? "X-Auth-User";

    /// <summary>
    /// When set to true, the application will not mask/censor sensitive data in the logs.
    /// </summary>
    public static bool IsUnmasked() => IsTrue(System.Environment.GetEnvironmentVariable(UnmaskedModeKey));

    /// <summary>
    /// When set to true, the application will log all environment variables set on startup
    /// </summary>
    /// <returns></returns>
    public static bool ShouldLogEnvVars() => IsTrue(System.Environment.GetEnvironmentVariable(LOG_ENV_VARS_KEY));

    /// <summary>
    /// Gets the configured Serilog log level from <c>LOG_LEVEL</c>. Defaults to <see cref="LogEventLevel.Debug"/>.
    /// </summary>
    public static LogEventLevel GetLogLevel()
    {
        var success = Enum.TryParse<LogEventLevel>(
            System.Environment.GetEnvironmentVariable(LOG_LEVEL_KEY),
            true,
            out var logLevel
        );

        return success ? logLevel : LogEventLevel.Debug;
    }

    /// <summary>
    /// Gets the application version from <c>INFORMATIONAL_VERSION</c> or <c>VERSION</c>. Defaults to <c>0.0.0</c>.
    /// </summary>
    public static string GetVersion() =>
        System.Environment.GetEnvironmentVariable(INFORMATIONAL_VERSION_KEY)
        ?? System.Environment.GetEnvironmentVariable(VERSION_KEY)
        ?? "0.0.0";

    /// <summary>
    /// Returns true if the current version indicates a development build (contains <c>dev</c>).
    /// </summary>
    public static bool IsDevRelease() => GetVersion().Contains("dev");

    /// <summary>
    /// Gets the process user ID (PUID) from the environment. Returns -1 when not set or invalid.
    /// </summary>
    public static int GetPuid() =>
        int.TryParse(System.Environment.GetEnvironmentVariable("PUID"), out var puid) ? puid : -1;

    /// <summary>
    /// Gets the process group ID (PGID) from the environment. Returns -1 when not set or invalid.
    /// </summary>
    public static int GetPgid() =>
        int.TryParse(System.Environment.GetEnvironmentVariable("PGID"), out var pgid) ? pgid : -1;

    /// <summary>
    /// Gets the port number from the DOTNET_HTTP_PORTS environment variable.
    /// </summary>
    /// <returns>The port number or 5000 if not configured.</returns>
    public static int GetPort =>
        int.TryParse(
            System
                .Environment.GetEnvironmentVariable("DOTNET_HTTP_PORTS")
                ?.Split(';', StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault(),
            out var port
        )
            ? port
            : 5000;

    /// <summary>
    /// Sets the <c>LOG_LEVEL</c> environment variable to the specified level (upper-cased).
    /// </summary>
    public static void SetLogLevel(LogEventLevel logLevel)
    {
        System.Environment.SetEnvironmentVariable(LOG_LEVEL_KEY, logLevel.ToString().ToUpper());
    }

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
}
