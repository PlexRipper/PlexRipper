using System.ComponentModel.DataAnnotations.Schema;

namespace PlexRipper.Domain;

public class PlexMediaSlim : BaseEntity
{
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
    /// This can be empty, in that case it gets the value of <see cref="Title"/>.
    /// </summary>
    [Column(Order = 4)]
    public required string SortTitle { get; init; }

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
    /// Gets or sets a value indicating whether this <see cref="PlexMedia"/> has a thumbnail.
    /// </summary>
    [Column(Order = 18)]
    public required bool HasThumb { get; set; }

    /// <summary>
    /// Gets or sets whether this <see cref="PlexMedia"/> has art.
    /// </summary>
    [Column(Order = 19)]
    public required bool HasArt { get; init; }

    /// <summary>
    /// Gets or sets whether this <see cref="PlexMedia"/> has a banner.
    /// </summary>
    [Column(Order = 20)]
    public required bool HasBanner { get; init; }

    /// <summary>
    /// Gets or sets whether this <see cref="PlexMedia"/> has a theme.
    /// </summary>
    [Column(Order = 21)]
    public required bool HasTheme { get; init; }

    public required PlexMediaContainer MediaData { get; init; }

    public required int PlexLibraryId { get; set; }

    public required int PlexServerId { get; set; }

    [NotMapped]
    public virtual PlexMediaType Type { get; init; }

    [NotMapped]
    public List<PlexMediaQuality> Qualities
    {
        get
        {
            return MediaData
                .MediaData.Select(x => new PlexMediaQuality()
                {
                    Quality = x.VideoResolution,
                    HashId = "NotImplementedYet",
                })
                .Reverse() // This sorts from lowest to highest quality
                .TakeLast(1) // TODO remove this when quality selector for downloading is implemented
                .ToList();
        }
    }

    #region Helpers

    [NotMapped]
    public string MetaDataUrl => $"/library/metadata/{Key}";

    [NotMapped]
    public string ThumbUrl => HasThumb ? $"{MetaDataUrl}/thumb/{MetaDataKey}" : string.Empty;

    [NotMapped]
    public string FullThumbUrl { get; set; } = string.Empty;

    [NotMapped]
    public string BannerUrl => HasBanner ? $"{MetaDataUrl}/banner/{MetaDataKey}" : string.Empty;

    [NotMapped]
    public string FullBannerUrl { get; init; } = string.Empty;

    [NotMapped]
    public string ArtUrl => HasArt ? $"{MetaDataUrl}/art/{MetaDataKey}" : string.Empty;

    [NotMapped]
    public string ThemeUrl => HasTheme ? $"{MetaDataUrl}/theme/{MetaDataKey}" : string.Empty;

    #endregion
}
