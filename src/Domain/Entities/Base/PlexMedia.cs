using System.ComponentModel.DataAnnotations.Schema;

namespace PlexRipper.Domain;

/// <summary>
///     Plex stores media in 1 generic type but PlexRipper stores it by type, this is the base entity for common
///     properties.
/// </summary>
public class PlexMedia : PlexMediaSlim
{
    #region Properties

    [Column(Order = 8)]
    public string Studio { get; set; } = string.Empty;

    [Column(Order = 9)]
    public string Summary { get; set; } = string.Empty;

    [Column(Order = 10)]
    public string ContentRating { get; set; } = string.Empty;

    [Column(Order = 11)]
    public double Rating { get; set; } = 0.0;

    /// <summary>
    /// Gets or sets when this media was released/aired to the public.
    /// </summary>
    [Column(Order = 15)]
    public DateTime? OriginallyAvailableAt { get; set; }

    [Column(Order = 16)]
    public int Index { get; set; } = -1;

    /// <summary>
    /// Gets or sets the full title path
    /// E.g. tvShow/Season/Episode
    /// TODO, might be better to remove this and make a getter for it.
    /// </summary>
    [Column(Order = 21)]
    public string FullTitle { get; set; } = string.Empty;

    #endregion

    #region Relationships

    public PlexLibrary? PlexLibrary { get; set; }

    public PlexServer? PlexServer { get; set; }

    #endregion
}
