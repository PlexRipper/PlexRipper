using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using PlexApi.Contracts;
using PlexRipper.PlexApi.Converters;
using PlexRipper.PlexApi.Helpers;

namespace PlexRipper.PlexApi.Models;

public class Metadata
{
    #region Movie

    /// <summary>
    /// Gets or sets the unique identifier for this metadata
    /// </summary>
    public string RatingKey { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier prefixed with the RatingKey for this metadata
    /// E.g: "/library/metadata/9718", where 9718 is the RatingKey
    /// </summary>
    public string Key { get; set; }

    public string LibrarySectionTitle { get; set; }

    public int LibrarySectionId { get; set; }

    public string LibrarySectionKey { get; set; }

    public string Studio { get; set; }

    public string Type { get; set; }

    public string Title { get; set; }

    public string ContentRating { get; set; }

    public string Summary { get; set; }

    public double Rating { get; set; }

    public double AudienceRating { get; set; }

    public int ViewCount { get; set; }

    public int LastViewedAt { get; set; }

    public int Year { get; set; }

    public string TagLine { get; set; }

    public string Thumb { get; set; }

    public string Art { get; set; }

    public int Duration { get; set; }

    public string OriginallyAvailableAt { get; set; }

    [JsonConverter(typeof(LongToDateTime))]
    public DateTime AddedAt { get; set; }

    [JsonConverter(typeof(LongToDateTime))]
    public DateTime UpdatedAt { get; set; }

    public string ChapterSource { get; set; }

    public string RatingImage { get; set; }

    public string ExternalProvider { get; set; }

    public string ExternalProviderId { get; set; }

    public string ShowOrdering { get; set; }

    public string AudienceRatingImage { get; set; }

    #endregion

    [JsonIgnore]
    public string Guid { get; set; }

    public string ExternalProviderInfo
    {
        set
        {
            var match = Regex.Match(value, @"\.(?<provider>[a-z]+)://(?<id>[^\?]+)");
            Guid = value;
            ExternalProvider = match.Groups["provider"].Value;
            ExternalProviderId = match.Groups["id"].Value;
        }
    }

    public List<Medium> Media { get; set; } = new();

    public List<Genre> Genres { get; set; } = new();

    public List<Director> Directors { get; set; } = new();

    public List<Writer> Writers { get; set; } = new();

    public List<Producer> Producer { get; set; } = new();

    public List<Country> Countries { get; set; } = new();

    public List<MediaRole> Roles { get; set; } = new();

    public List<Similar> Similar { get; set; } = new();

    public List<Field> Field { get; set; } = new();

    #region Library Sections

    public string TitleSort { get; set; }

    public int Index { get; set; }

    public string Banner { get; set; }

    public int LeafCount { get; set; }

    public int ViewedLeafCount { get; set; }

    [JsonConverter(typeof(IntValueConverter))]
    public int ChildCount { get; set; }

    public string Theme { get; set; }

    [JsonConverter(typeof(StringToBool))]
    public bool FlattenSeasons { get; set; }

    public int? SkipCount { get; set; }

    #endregion

    #region TV Show Seasons

    public string ParentRatingKey { get; set; }

    public string ParentKey { get; set; }

    public string ParentTitle { get; set; }

    public int ParentIndex { get; set; }

    public string ParentThumb { get; set; }

    public string ParentTheme { get; set; }

    #endregion

    #region TV Show Episode

    public string GrandparentRatingKey { get; set; }

    public string GrandparentKey { get; set; }

    public string GrandparentTitle { get; set; }

    public string GrandparentThumb { get; set; }

    public string GrandparentArt { get; set; }

    public string GrandparentTheme { get; set; }

    #endregion

    #region Movie Section

    public string PrimaryExtraKey { get; set; }

    public List<Collection> Collection { get; set; }

    public string OriginalTitle { get; set; }

    public int? ViewOffset { get; set; }

    #endregion
}