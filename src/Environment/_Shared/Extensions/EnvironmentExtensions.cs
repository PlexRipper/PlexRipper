using Serilog.Events;

namespace Reaparr.Environment;

public static class EnvironmentExtensions
{
    private static readonly AsyncLocal<IReadOnlyDictionary<string, string?>?> _testOverrides = new();


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
