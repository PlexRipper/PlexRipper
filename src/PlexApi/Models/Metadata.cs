using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using PlexRipper.PlexApi.Converters;
using PlexRipper.PlexApi.Helpers;

namespace PlexRipper.PlexApi.Models;

public class Metadata
{
    #region Movie

    [JsonPropertyName("ratingKey")]
    public string RatingKey { get; set; }

    [JsonPropertyName("key")]
    public string Key { get; set; }

    [JsonPropertyName("librarySectionTitle")]
    public string LibrarySectionTitle { get; set; }

    [JsonPropertyName("librarySectionId")]
    public int LibrarySectionId { get; set; }

    [JsonPropertyName("librarySectionKey")]
    public string LibrarySectionKey { get; set; }

    [JsonPropertyName("studio")]
    public string Studio { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("contentRating")]
    public string ContentRating { get; set; }

    [JsonPropertyName("summary")]
    public string Summary { get; set; }

    [JsonPropertyName("rating")]
    public double Rating { get; set; }

    [JsonPropertyName("audienceRating")]
    public double AudienceRating { get; set; }

    [JsonPropertyName("viewCount")]
    public int ViewCount { get; set; }

    [JsonPropertyName("lastViewedAt")]
    public int LastViewedAt { get; set; }

    [JsonPropertyName("year")]
    public int Year { get; set; }

    [JsonPropertyName("tagLine")]
    public string TagLine { get; set; }

    [JsonPropertyName("thumb")]
    public string Thumb { get; set; }

    [JsonPropertyName("art")]
    public string Art { get; set; }

    [JsonPropertyName("duration")]
    public int Duration { get; set; }

    [JsonPropertyName("originallyAvailableAt")]
    public string OriginallyAvailableAt { get; set; }

    [JsonPropertyName("addedAt")]
    [JsonConverter(typeof(LongToDateTime))]
    public DateTime AddedAt { get; set; }

    [JsonPropertyName("updatedAt")]
    [JsonConverter(typeof(LongToDateTime))]
    public DateTime UpdatedAt { get; set; }

    [JsonPropertyName("chapterSource")]
    public string ChapterSource { get; set; }

    [JsonPropertyName("ratingImage")]
    public string RatingImage { get; set; }

    [JsonPropertyName("externalProvider")]
    public string ExternalProvider { get; set; }

    [JsonPropertyName("externalProviderId")]
    public string ExternalProviderId { get; set; }

    [JsonPropertyName("showOrdering")]
    public string ShowOrdering { get; set; }

    [JsonPropertyName("audienceRatingImage")]
    public string AudienceRatingImage { get; set; }

    #endregion

    [JsonIgnore]
    public string Guid { get; set; }

    [JsonPropertyName("guid")]
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

    [JsonPropertyName("Media")]
    public List<Medium> Media { get; set; }

    [JsonPropertyName("Genre")]
    public Genre[] Genres { get; set; }

    [JsonPropertyName("Director")]
    public Director[] Directors { get; set; }

    [JsonPropertyName("Writer")]
    public Writer[] Writers { get; set; }

    [JsonPropertyName("Producer")]
    public List<Producer> Producer { get; set; }

    [JsonPropertyName("Country")]
    public Country[] Countries { get; set; }

    [JsonPropertyName("Role")]
    public List<MediaRole> Roles { get; set; }

    [JsonPropertyName("Similar")]
    public List<Similar> Similar { get; set; }

    [JsonPropertyName("Field")]
    public List<Field> Field { get; set; }

    #region Library Sections

    [JsonPropertyName("TitleSort")]
    public string TitleSort { get; set; }

    [JsonPropertyName("Index")]
    public int Index { get; set; }

    [JsonPropertyName("Banner")]
    public string Banner { get; set; }

    [JsonPropertyName("LeafCount")]
    public int LeafCount { get; set; }

    [JsonPropertyName("ViewedLeafCount")]
    public int ViewedLeafCount { get; set; }

    [JsonPropertyName("ChildCount")]
    [JsonConverter(typeof(IntValueConverter))]
    public int ChildCount { get; set; }

    [JsonPropertyName("Theme")]
    public string Theme { get; set; }

    [JsonPropertyName("flattenSeasons")]
    [JsonConverter(typeof(StringToBool))]
    public bool FlattenSeasons { get; set; }

    [JsonPropertyName("skipCount")]
    public int? SkipCount { get; set; }

    #endregion

    #region TV Show Seasons

    [JsonPropertyName("parentRatingKey")]
    public string ParentRatingKey { get; set; }

    [JsonPropertyName("parentKey")]
    public string ParentKey { get; set; }

    [JsonPropertyName("parentTitle")]
    public string ParentTitle { get; set; }

    [JsonPropertyName("parentIndex")]
    public int ParentIndex { get; set; }

    [JsonPropertyName("parentThumb")]
    public string ParentThumb { get; set; }

    [JsonPropertyName("parentTheme")]
    public string ParentTheme { get; set; }

    #endregion

    #region TV Show Episode

    [JsonPropertyName("grandparentRatingKey")]
    public string GrandparentRatingKey { get; set; }

    [JsonPropertyName("grandparentKey")]
    public string GrandparentKey { get; set; }

    [JsonPropertyName("grandparentTitle")]
    public string GrandparentTitle { get; set; }

    [JsonPropertyName("grandparentThumb")]
    public string GrandparentThumb { get; set; }

    [JsonPropertyName("grandparentArt")]
    public string GrandparentArt { get; set; }

    [JsonPropertyName("grandparentTheme")]
    public string GrandparentTheme { get; set; }

    #endregion

    #region Movie Section

    [JsonPropertyName("primaryExtraKey")]
    public string PrimaryExtraKey { get; set; }

    [JsonPropertyName("collection")]
    public List<Collection> Collection { get; set; }

    [JsonPropertyName("originalTitle")]
    public string OriginalTitle { get; set; }

    [JsonPropertyName("viewOffset")]
    public int? ViewOffset { get; set; }

    #endregion
}