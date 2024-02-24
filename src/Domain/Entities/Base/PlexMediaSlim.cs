using System.ComponentModel.DataAnnotations.Schema;

namespace PlexRipper.Domain;

public class PlexMediaSlim : BaseEntity
{
    /// <summary>
    /// Unique key identifying this item by the Plex Api. This is used by the PlexServers to differentiate between media items.
    /// e.g: 28550, 1723, 21898.
    /// </summary>
    [Column(Order = 1)]
    public int Key { get; set; }

    [Column(Order = 2)]
    public string Title { get; set; }

    [Column(Order = 3)]
    public int Year { get; set; }

    /// <summary>
    /// This can be empty, in that case it gets the value of <see cref="Title"/>.
    /// </summary>
    [Column(Order = 4)]
    public string SortTitle { get; set; }

    /// <summary>
    /// Gets or sets the duration in seconds of the (nested) media.
    /// </summary>
    [Column(Order = 5)]
    public int Duration { get; set; }

    /// <summary>
    /// Gets or sets the total filesize of the nested media.
    /// </summary>
    [Column(Order = 6)]
    public long MediaSize { get; set; }

    /// <summary>
    /// Gets or sets the key used to retrieve thumbnails, art or banners.
    /// E.g. /library/metadata/[Key]/art/[MetadataKey] =>  /library/metadata/529367/art/1593898227.
    /// </summary>
    [Column(Order = 7)]
    public int MetaDataKey { get; set; }

    /// <summary>
    /// Gets or sets the number of direct children
    /// E.G. if the type is tvShow, then this number would be the season count, if season then this would be the episode count.
    /// </summary>
    [Column(Order = 12)]
    public int ChildCount { get; set; }

    /// <summary>
    /// Gets or sets when this media was added to the Plex library.
    /// </summary>
    [Column(Order = 13)]
    public DateTime AddedAt { get; set; }

    /// <summary>
    /// Gets or sets when this media was last updated in the Plex library.
    /// </summary>
    [Column(Order = 14)]
    public DateTime UpdatedAt { get; set; }

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

    [Column(Order = 22)]
    public PlexMediaContainer MediaData { get; set; } = new();

    public int PlexLibraryId { get; set; }

    public int PlexServerId { get; set; }

    [NotMapped]
    public virtual PlexMediaType Type { get; set; }

    [NotMapped]
    public List<PlexMediaQuality> Qualities
    {
        get
        {
            return MediaData.MediaData.Select(x => new PlexMediaQuality()
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
    public string FullThumbUrl { get; set; }

    [NotMapped]
    public string BannerUrl => HasBanner ? $"{MetaDataUrl}/banner/{MetaDataKey}" : string.Empty;

    [NotMapped]
    public string FullBannerUrl { get; set; }

    [NotMapped]
    public string ArtUrl => HasArt ? $"{MetaDataUrl}/art/{MetaDataKey}" : string.Empty;

    [NotMapped]
    public string ThemeUrl => HasTheme ? $"{MetaDataUrl}/theme/{MetaDataKey}" : string.Empty;

    #endregion
}