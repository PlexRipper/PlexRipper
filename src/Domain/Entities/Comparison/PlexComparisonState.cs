using System.ComponentModel.DataAnnotations;

namespace Reaparr.Domain;

/// <summary>
/// Records that a remote library has been compared against an owned library for a specific media type.
/// Without this scope, absence of hit rows means "not compared yet", not "missing".
/// </summary>
public class PlexComparisonState : BaseEntity
{
    /// <summary>
    /// Remote library source.
    /// </summary>
    public required int RemotePlexLibraryId { get; init; }

    /// <summary>
    /// Owned library target.
    /// </summary>
    public required int OwnedPlexLibraryId { get; init; }

    /// <summary>
    /// Media type compared (Movie, TvShow, Season, Episode, or later Music).
    /// </summary>
    public required PlexMediaType MediaType { get; init; }

    /// <summary>
    /// When this comparison completed.
    /// </summary>
    public required DateTime CompletedAt { get; set; }

    /// <summary>
    /// Remote library update timestamp captured when this comparison completed.
    /// </summary>
    public DateTime? RemoteLibraryUpdatedAt { get; set; }

    /// <summary>
    /// Owned library update timestamp captured when this comparison completed.
    /// </summary>
    public DateTime? OwnedLibraryUpdatedAt { get; set; }

    public PlexLibrary? RemotePlexLibrary { get; init; }

    public PlexLibrary? OwnedPlexLibrary { get; init; }
}
