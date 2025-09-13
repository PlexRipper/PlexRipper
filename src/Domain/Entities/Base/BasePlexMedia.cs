namespace Reaparr.Domain;

/// <summary>
///     Plex stores media in 1 generic type but Reaparr stores it by type, this is the base entity for common
///     properties.
/// </summary>
public class BasePlexMedia : BaseEntity
{
    #region Properties

    /// <summary>
    /// Unique key identifying this item by the Plex Api. This is used by the PlexServers to differentiate between media items.
    /// e.g: 28550, 1723, 21898.
    /// </summary>
    [Column(Order = 1)]
    public required int Key { get; set; }

    [Column(Order = 2)]
    public required string Title { get; set; }

    [Column(Order = 3)]
    public required int Year { get; set; }

    /// <summary>
    /// Gets or sets the sort index of the media based on the abc sort order. This makes sorting much quicker as it can sort on this index which is unique within a <see cref="PlexLibrary"/>.
    /// </summary>
    [Column(Order = 4)]
    public required int SortIndex { get; set; }

    [Column(Order = 5)]
    public required string SearchTitle { get; init; }

    /// <summary>
    /// Gets or sets the duration in seconds of the (nested) media.
    /// </summary>
    [Column(Order = 6)]
    public required int Duration { get; set; }

    /// <summary>
    /// Gets or sets the total filesize of the nested media.
    /// </summary>
    [Column(Order = 7)]
    public required long MediaSize { get; set; }

    /// <summary>
    /// Gets or sets the key used to retrieve thumbnails, art or banners.
    /// E.g. /library/metadata/[Key]/art/[MetadataKey] =>  /library/metadata/529367/art/1593898227.
    /// </summary>
    [Column(Order = 8)]
    public required int MetaDataKey { get; init; }

    [Column(Order = 9)]
    public required string Studio { get; init; } = string.Empty;

    [Column(Order = 10)]
    public required string Summary { get; init; } = string.Empty;

    [Column(Order = 11)]
    public required string ContentRating { get; init; } = string.Empty;

    [Column(Order = 12)]
    public required double Rating { get; init; }

    /// <summary>
    /// Gets or sets the number of direct children
    /// E.G. if the type is tvShow, then this number would be the season count, if season then this would be the episode count.
    /// </summary>
    [Column(Order = 13)]
    public required int ChildCount { get; set; }

    /// <summary>
    /// Gets or sets when this media was added to the Plex library.
    /// </summary>
    [Column(Order = 14)]
    public required DateTime AddedAt { get; init; }

    /// <summary>
    /// Gets or sets when this media was last updated in the Plex library.
    /// </summary>
    [Column(Order = 15)]
    public required DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets when this media was released/aired to the public.
    /// </summary>
    [Column(Order = 16)]
    public required DateTime? OriginallyAvailableAt { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="BasePlexMedia"/> has a thumbnail.
    /// </summary>
    [Column(Order = 18)]
    public required bool HasThumb { get; set; }

    /// <summary>
    /// Gets or sets whether this <see cref="BasePlexMedia"/> has art / banner.
    /// </summary>
    [Column(Order = 19)]
    public required bool HasArt { get; init; }

    /// <summary>
    /// Gets or sets whether this <see cref="BasePlexMedia"/> has a theme.
    /// </summary>
    [Column(Order = 21)]
    public required bool HasTheme { get; init; }

    /// <summary>
    /// Gets or sets the full title path
    /// E.g. tvShow/Season/Episode
    /// TODO, might be better to remove this and make a getter for it.
    /// </summary>
    [Column(Order = 22)]
    public required string FullTitle { get; set; } = string.Empty;

    [Column(Order = 23)]
    public required string Guid { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the IMDB guid.
    /// Note: this is only the unique identifier part, and not including "imdb://".
    /// <example>imdb://imdb0397306</example>
    /// </summary>
    [Column(Order = 24)]
    public required string? Guid_IMDB { get; init; }

    /// <summary>
    /// Gets or sets the TMDB guid.
    /// Note: this is only the unique identifier part, and not including "tmdb://".
    /// <example>tmdb://1433</example>
    /// </summary>
    [Column(Order = 25)]
    public required int? Guid_TMDB { get; init; }

    /// <summary>
    /// Gets or sets the TVDB guid.
    /// Note: this is only the unique identifier part, and not including "tvdb://".
    /// <example>tvdb://73141</example>
    /// </summary>
    [Column(Order = 26)]
    public required int? Guid_TVDB { get; init; }

    #endregion

    #region Relationships

    public required int PlexLibraryId { get; set; }

    public required int PlexServerId { get; set; }

    public PlexLibrary? PlexLibrary { get; set; }

    public PlexServer? PlexServer { get; init; }

    #endregion

    #region Helpers

    [NotMapped]
    public virtual PlexMediaType Type { get; init; }

    [NotMapped]
    public string MetaDataUrl => $"/library/metadata/{Key}";

    [NotMapped]
    public string ThumbUrl => HasThumb ? $"{MetaDataUrl}/thumb/{MetaDataKey}" : string.Empty;

    [NotMapped]
    public string FullBannerUrl { get; init; } = string.Empty;

    [NotMapped]
    public string ArtUrl => HasArt ? $"{MetaDataUrl}/art/{MetaDataKey}" : string.Empty;

    [NotMapped]
    public string ThemeUrl => HasTheme ? $"{MetaDataUrl}/theme/{MetaDataKey}" : string.Empty;

    #endregion
}
