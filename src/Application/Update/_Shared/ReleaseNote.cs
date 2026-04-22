namespace Reaparr.Application;

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
    /// Markdown-formatted release notes; empty string when not provided.
    /// </summary>
    public required string Notes { get; init; }

    public required DateTime ReleaseDate { get; init; }

    public required bool IsDevRelease { get; init; }
}
