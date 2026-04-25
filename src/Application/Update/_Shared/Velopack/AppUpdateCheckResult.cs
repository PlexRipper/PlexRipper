namespace Reaparr.Application;

/// <summary>
/// Represents the result of checking whether an application update is available.
/// </summary>
public sealed record AppUpdateCheckResult
{
    /// <summary>
    /// Whether a newer application version is available.
    /// </summary>
    public required bool IsUpdateAvailable { get; init; }

    /// <summary>
    /// The latest available version when an update exists; otherwise the current version.
    /// </summary>
    public required string NewestVersion { get; init; }

    /// <summary>
    /// Gets the current Reaparr version
    /// </summary>
    public required string CurrentVersion { get; init; }

    /// <summary>
    /// All release notes for versions between the current installation and <see cref="NewestVersion"/>, newest first.
    /// Empty when no update is available.
    /// </summary>
    public required IReadOnlyList<ReleaseNote> ReleaseNotes { get; init; }
}
