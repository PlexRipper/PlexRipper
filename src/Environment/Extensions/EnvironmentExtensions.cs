using Serilog.Events;

namespace Environment;

public static class EnvironmentExtensions
{
    public const string IntegrationTestModeKey = "IntegrationTestMode";

    public const string UnmaskedModeKey = "UNMASKED";

    public const string LogEnvVarsKey = "LOG_ENV_VARS";

    public const string LogLevelKey = "LOG_LEVEL";

    public const string VersionKey = "VERSION";

    public const string InformationalVersionKey = "INFORMATIONAL_VERSION";

    public const string DevelopmentRootPathKey = "DEVELOPMENT_ROOT_PATH";

    private static readonly string TrueValue = Convert.ToString(true);

    /// <summary>
    /// Determines if the value is true.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private static bool IsTrue(string? value) =>
        value == Convert.ToString(true) || value == "1" || value == "true" || value == "TRUE";

    public static bool IsIntegrationTestMode() =>
        System.Environment.GetEnvironmentVariable(IntegrationTestModeKey) == TrueValue;

    /// <summary>
    /// This is the path that is used to store the /config, /downloads, /movies and /tvshows folders required to boot PlexRipper in development mode in a non-docker environment.
    /// </summary>
    /// <returns></returns>
    public static string? GetDevelopmentRootPath() => System.Environment.GetEnvironmentVariable(DevelopmentRootPathKey);

    /// <summary>
    /// When set to true, the application will not mask/censor sensitive data in the logs.
    /// </summary>
    public static bool IsUnmasked() => IsTrue(System.Environment.GetEnvironmentVariable(UnmaskedModeKey));

    /// <summary>
    /// When set to true, the application will log all environment variables set on startup
    /// </summary>
    /// <returns></returns>
    public static bool ShouldLogEnvVars() => IsTrue(System.Environment.GetEnvironmentVariable(LogEnvVarsKey));

    public static LogEventLevel GetLogLevel()
    {
        var success = Enum.TryParse<LogEventLevel>(
            System.Environment.GetEnvironmentVariable(LogLevelKey),
            true,
            out var logLevel
        );

        return success ? logLevel : LogEventLevel.Debug;
    }

    public static string GetVersion() =>
        System.Environment.GetEnvironmentVariable(InformationalVersionKey)
        ?? System.Environment.GetEnvironmentVariable(VersionKey)
        ?? "0.0.0";

    public static bool IsDevRelease() => GetVersion().Contains("dev");

    public static int GetPuid() => int.Parse(System.Environment.GetEnvironmentVariable("PUID") ?? "-1");

    public static int GetPgid() => int.Parse(System.Environment.GetEnvironmentVariable("PGID") ?? "-1");

    public static void SetLogLevel(LogEventLevel logLevel)
    {
        System.Environment.SetEnvironmentVariable(LogLevelKey, logLevel.ToString().ToUpper());
    }

    public static void SetIntegrationTestMode(bool state)
    {
        System.Environment.SetEnvironmentVariable(IntegrationTestModeKey, state.ToString());
    }

    /// <summary>
    /// When set to true, the application will not mask/censor sensitive data in the logs.
    /// </summary>
    public static void SetUnmaskedLogMode(bool state)
    {
        System.Environment.SetEnvironmentVariable(UnmaskedModeKey, state.ToString());
    }
}
