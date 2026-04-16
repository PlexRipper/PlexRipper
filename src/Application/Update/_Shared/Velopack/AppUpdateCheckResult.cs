namespace Reaparr.Application;

/// <summary>
/// Represents the result of checking whether an application update is available.
/// </summary>
public sealed record AppUpdateCheckResult
{
    /// <summary>
    /// Creates a result representing that no update is available.
    /// </summary>
    public static AppUpdateCheckResult NoUpdate() =>
        new()
        {
            IsUpdateAvailable = false,
            NewestVersion = EnvironmentExtensions.GetVersion(),
            ReleaseNotes = [],
            CurrentVersion = EnvironmentExtensions.GetVersion(),
        };

    /// <summary>
    /// Creates a result representing that an update is available.
    /// </summary>
    /// <param name="availableVersion">The latest available version.</param>
    /// <param name="releaseNotes">All release notes since the currently installed version, newest first.</param>
    public static AppUpdateCheckResult UpdateAvailable(
        string availableVersion,
        IReadOnlyList<ReleaseNote> releaseNotes
    ) =>
        new()
        {
            IsUpdateAvailable = true,
            NewestVersion = availableVersion,
            ReleaseNotes = releaseNotes,
            CurrentVersion = EnvironmentExtensions.GetVersion(),
        };

    /// <summary>
    /// Whether a newer application version is available.
    /// </summary>
    public required bool IsUpdateAvailable { get; init; }

    /// <summary>
    /// The latest available version when an update exists; otherwise <c>null</c>.
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

/// <summary>
/// A single release entry containing version and markdown release notes.
/// </summary>
public sealed record ReleaseNote
{
    /// <summary>
    /// The release version string (e.g. "1.2.3").
    /// </summary>
    public required string Version { get; init; }

    /// <summary>
    /// Markdown-formatted release notes, or <c>null</c> when not provided.
    /// </summary>
    public required string? Notes { get; init; }
}
