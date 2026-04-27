using Serilog.Events;

namespace Reaparr.Environment;

public static class EnvironmentExtensions
{
    private static readonly AsyncLocal<IReadOnlyDictionary<string, string?>?> _testOverrides = new();

    #region Setters

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

    #endregion
}