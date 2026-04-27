using Serilog.Events;

namespace Reaparr.Environment;

public static class EnvironmentExtensions
{
    private static readonly AsyncLocal<IReadOnlyDictionary<string, string?>?> _testOverrides = new();

    #region Getters

    /// <summary>
    /// Returns true when <c>IntegrationTestMode</c> environment variable is set to a truthy value.
    /// </summary>
    public static bool IsIntegrationTestMode() => IsTrue(GetEnvironmentVariable(EnvKeys.IntegrationTestMode));

    public static string? GetAppImage() => GetEnvironmentVariable(EnvKeys.AppImage);

    /// <summary>
    /// Gets the name of the HTTP header used for bearer/auth token passing. Defaults to <c>X-Auth-User</c>.
    /// </summary>
    public static string GetHeaderAuthTokenName() =>
        GetEnvironmentVariable(EnvKeys.AuthHeaderTokenName) ?? "X-Auth-User";

    /// <summary>
    /// When set to true, the application will not mask/censor sensitive data in the logs.
    /// </summary>
    public static bool IsUnmasked() => IsTrue(GetEnvironmentVariable(EnvKeys.Unmasked));

    /// <summary>
    /// When set to true, the application will log all environment variables set on startup
    /// </summary>
    public static bool ShouldLogEnvVars() => IsTrue(GetEnvironmentVariable(EnvKeys.LogEnvironmentVariables));

    /// <summary>
    /// Gets the configured Serilog log level from <c>LOG_LEVEL</c>. Defaults to <see cref="LogEventLevel.Debug"/>.
    /// </summary>
    public static LogEventLevel GetLogLevel()
    {
        var success = Enum.TryParse<LogEventLevel>(GetEnvironmentVariable(EnvKeys.LogLevel), true, out var logLevel);
        return success ? logLevel : LogEventLevel.Debug;
    }

    /// <summary>
    /// Returns true if the DOTNET_ENVIRONMENT is set to Development.
    /// </summary>
    /// <returns></returns>
    public static bool IsDevelopmentEnvironment() => GetEnvironmentVariable(EnvKeys.DotNetEnvironment) == "Development";

    /// <summary>
    /// When set to a truthy value, disables all authentication. FOR DEVELOPMENT USE ONLY.
    /// </summary>
    public static bool IsAuthenticationDisabled() => IsTrue(GetEnvironmentVariable(EnvKeys.DisableAuthentication));

    /// <summary>
    /// Gets the process user ID (PUID) from the environment. Returns -1 when not set or invalid.
    /// </summary>
    public static int GetPuid() => int.TryParse(GetEnvironmentVariable(EnvKeys.Puid), out var puid) ? puid : -1;

    /// <summary>
    /// Gets the process group ID (PGID) from the environment. Returns -1 when not set or invalid.
    /// </summary>
    public static int GetPgid() => int.TryParse(GetEnvironmentVariable(EnvKeys.Pgid), out var pgid) ? pgid : -1;

    /// <summary>
    /// Gets the port number from the DOTNET_HTTP_PORTS environment variable.
    /// </summary>
    /// <returns>The port number or 5000 if not configured.</returns>
    public static int GetPort => int.TryParse(
        GetEnvironmentVariable(EnvKeys.DotNetHttpPorts)
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
    public static string GetSeqUrl() => GetEnvironmentVariable(EnvKeys.SeqUrl) ?? "http://localhost:5341";

    #endregion

    #region Setters

    public static void SetPort(int port) =>
        System.Environment.SetEnvironmentVariable(EnvKeys.DotNetHttpPorts, port.ToString());

    /// <summary>
    /// Sets the <c>LOG_LEVEL</c> environment variable to the specified level (upper-cased).
    /// </summary>
    public static void SetLogLevel(LogEventLevel logLevel)
    {
        System.Environment.SetEnvironmentVariable(EnvKeys.LogLevel, logLevel.ToString().ToUpper());
    }

    /// <summary>
    /// Enables or disables integration test mode by setting <c>IntegrationTestMode</c>.
    /// </summary>
    public static void SetIntegrationTestMode(bool state)
    {
        System.Environment.SetEnvironmentVariable(EnvKeys.IntegrationTestMode, state.ToString());
    }

    /// <summary>
    /// When set to true, the application will not mask/censor sensitive data in the logs.
    /// </summary>
    public static void EnableUnmaskedLog(bool state)
    {
        System.Environment.SetEnvironmentVariable(EnvKeys.Unmasked, state.ToString());
    }

    /// <summary>
    /// When set to true, the application will log all environment variables set on startup.
    /// </summary>
    public static void EnableLogEnvVars(bool state)
    {
        System.Environment.SetEnvironmentVariable(EnvKeys.LogEnvironmentVariables, state.ToString());
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Sets per-async-flow environment variable overrides for the duration of the returned scope.
    /// Intended for use in tests only. Each async flow (test) gets its own isolated slot via <see cref="AsyncLocal{T}"/>.
    /// </summary>
    internal static IDisposable WithOverrides(IReadOnlyDictionary<string, string?> overrides)
    {
        var previous = _testOverrides.Value;
        _testOverrides.Value = overrides;
        return new OverrideScope(previous);
    }

    private sealed class OverrideScope(IReadOnlyDictionary<string, string?>? previous) : IDisposable
    {
        public void Dispose() => _testOverrides.Value = previous;
    }

    public static string? GetEnvironmentVariable(string key)
    {
        if (_testOverrides.Value is { } overrides && overrides.TryGetValue(key, out var val))
            return string.IsNullOrWhiteSpace(val) ? null : val.Trim();

        var value = System.Environment.GetEnvironmentVariable(key)?.Trim();
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    /// <summary>
    /// Determines if the value is true.
    /// </summary>
    /// <param name="value"></param>
    private static bool IsTrue(string? value) => value is not null
                                                 && (string.Equals(value, Convert.ToString(true),
                                                     StringComparison.OrdinalIgnoreCase) || value == "1");

    #endregion
}