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
    /// Unique key identifying this item by the Plex Api. This is used by the PlexServers to differentiate between media items.
    /// e.g: 28550, 1723, 21898.
    /// </summary>
    [Column(Order = 1)]
    public int Key { get; set; }

    /// <summary>
    /// Gets or sets the key used to retrieve thumbnails, art or banners.
    /// E.g. /library/metadata/[Key]/art/[MetadataKey] =>  /library/metadata/529367/art/1593898227.
    /// </summary>
    [Column(Order = 7)]
    public int MetaDataKey { get; set; }

    [Column(Order = 8)]
    public string Studio { get; set; }

    [Column(Order = 9)]
    public string Summary { get; set; }

    [Column(Order = 10)]
    public string ContentRating { get; set; }

    [Column(Order = 11)]
    public double Rating { get; set; }

    /// <summary>
    /// Gets or sets when this media was released/aired to the public.
    /// </summary>
    [Column(Order = 15)]
    public DateTime? OriginallyAvailableAt { get; set; }

    [Column(Order = 16)]
    public int Index { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="PlexMedia"/> has a thumbnail.
    /// </summary>
    [Column(Order = 17)]
    public bool HasThumb { get; set; }

    /// <summary>
    /// Gets or sets whether this <see cref="PlexMedia"/> has art.
    /// </summary>
    [Column(Order = 18)]
    public bool HasArt { get; set; }

    /// <summary>
    /// Gets or sets whether this <see cref="PlexMedia"/> has a banner.
    /// </summary>
    [Column(Order = 19)]
    public bool HasBanner { get; set; }

    /// <summary>
    /// Gets or sets whether this <see cref="PlexMedia"/> has a theme.
    /// </summary>
    [Column(Order = 20)]
    public bool HasTheme { get; set; }

    /// <summary>
    /// Gets or sets the full title path
    /// E.g. tvShow/Season/Episode
    /// TODO, might be better to remove this and make a getter for it.
    /// </summary>
    [Column(Order = 21)]
    public string FullTitle { get; set; }

    [Column(Order = 22)]
    public PlexMediaContainer MediaData { get; set; } = new();

    #endregion

    #region Relationships

    public PlexLibrary PlexLibrary { get; set; }

    public PlexServer PlexServer { get; set; }

    #endregion

    #region Helpers

    [NotMapped]
    public string MetaDataUrl => $"/library/metadata/{Key}";

    [NotMapped]
    public string ThumbUrl => HasThumb ? $"{MetaDataUrl}/thumb/{MetaDataKey}" : string.Empty;

    [NotMapped]
    public string BannerUrl => HasBanner ? $"{MetaDataUrl}/banner/{MetaDataKey}" : string.Empty;

    [NotMapped]
    public string ArtUrl => HasArt ? $"{MetaDataUrl}/art/{MetaDataKey}" : string.Empty;

    [NotMapped]
    public string ThemeUrl => HasTheme ? $"{MetaDataUrl}/theme/{MetaDataKey}" : string.Empty;

    #endregion
}