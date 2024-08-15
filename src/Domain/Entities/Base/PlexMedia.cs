using System.ComponentModel.DataAnnotations.Schema;

namespace PlexRipper.Domain;

/// <summary>
///     Plex stores media in 1 generic type but PlexRipper stores it by type, this is the base entity for common
///     properties.
/// </summary>
public class PlexMedia : PlexMediaSlim
{
    #region Properties

    /// <summary>
    /// This can be empty, in that case it gets the value of <see cref="Title"/>.
    /// </summary>
    [Column(Order = 5)]
    public required string SearchTitle { get; init; }

    [Column(Order = 9)]
    public string Studio { get; init; } = string.Empty;

    [Column(Order = 10)]
    public string Summary { get; init; } = string.Empty;

    [Column(Order = 11)]
    public string ContentRating { get; init; } = string.Empty;

    [Column(Order = 12)]
    public double Rating { get; init; } = 0.0;

    /// <summary>
    /// Gets or sets when this media was released/aired to the public.
    /// </summary>
    [Column(Order = 16)]
    public DateTime? OriginallyAvailableAt { get; init; }

    [Column(Order = 17)]
    public int Index { get; init; } = -1;

    /// <summary>
    /// Gets or sets the full title path
    /// E.g. tvShow/Season/Episode
    /// TODO, might be better to remove this and make a getter for it.
    /// </summary>
    [Column(Order = 22)]
    public string FullTitle { get; set; } = string.Empty;

    [Column(Order = 23)]
    public string Guid { get; init; } = string.Empty;

    #endregion

    #region Relationships

    public PlexLibrary? PlexLibrary { get; set; }

    public PlexServer? PlexServer { get; init; }

    #endregion
}
