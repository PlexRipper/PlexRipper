namespace Reaparr.Application.Contracts;

public record AppUpdateCheckDTO
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
    public required IReadOnlyList<ReleaseNoteDTO> ReleaseNotes { get; init; }
}

/// <summary>
/// A single release entry containing version and markdown release notes.
/// </summary>
public sealed record ReleaseNoteDTO
{
    /// <summary>
    /// The release version string (e.g. "1.2.3").
    /// </summary>
    public required string Version { get; init; }

    /// <summary>
    /// Markdown-formatted release notes; empty string when not provided.
    /// </summary>
    public required string Notes { get; init; }

    public required DateTime ReleaseDate { get; init; }

    public required bool IsDevRelease { get; init; }
}
