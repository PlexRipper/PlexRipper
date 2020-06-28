using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PlexRipper.PlexApi.Common.DTO.PlexLibraryMedia
{
    /// <summary>
    /// This DTO is used within PlexApi => GetLibraryMediaAsync. It returns the Metadata for all media inside a PlexLibrary
    /// </summary>
    public class PlexLibraryMediaDTO
    {
        [JsonPropertyName("MediaContainer")]
        public PlexLibraryMediaContainerDTO MediaContainer { get; set; }
    }

    public class PlexLibraryMediaContainerDTO
    {

        [JsonPropertyName("size")]
        public int Size { get; set; }

        [JsonPropertyName("allowSync")]
        public bool AllowSync { get; set; }

        [JsonPropertyName("art")]
        public string Art { get; set; }

        [JsonPropertyName("identifier")]
        public string Identifier { get; set; }

        [JsonPropertyName("librarySectionID")]
        public int LibrarySectionID { get; set; }

        [JsonPropertyName("librarySectionTitle")]
        public string LibrarySectionTitle { get; set; }

        [JsonPropertyName("librarySectionUUID")]
        public string LibrarySectionUUID { get; set; }

        [JsonPropertyName("mediaTagPrefix")]
        public string MediaTagPrefix { get; set; }

        [JsonPropertyName("mediaTagVersion")]
        public int MediaTagVersion { get; set; }

        [JsonPropertyName("nocache")]
        public bool Nocache { get; set; }

        [JsonPropertyName("thumb")]
        public string Thumb { get; set; }

        [JsonPropertyName("title1")]
        public string Title1 { get; set; }

        [JsonPropertyName("title2")]
        public string Title2 { get; set; }

        [JsonPropertyName("viewGroup")]
        public string ViewGroup { get; set; }

        [JsonPropertyName("viewMode")]
        public int ViewMode { get; set; }

        [JsonPropertyName("Metadata")]
        public IList<PlexLibraryMetaDataDTO> Metadata { get; set; }

    }

    /// <summary>
    /// The media container for the media metadata, this is the same for movies, tv shows and music
    /// </summary>
    public class PlexLibraryMetaDataDTO
    {

        [JsonPropertyName("ratingKey")]
        public string RatingKey { get; set; }

        [JsonPropertyName("key")]
        public string Key { get; set; }

        [JsonPropertyName("guid")]
        public string Guid { get; set; }

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

        [JsonPropertyName("index")]
        public int Index { get; set; }

        [JsonPropertyName("rating")]
        public double Rating { get; set; }

        [JsonPropertyName("year")]
        public int Year { get; set; }

        [JsonPropertyName("thumb")]
        public string Thumb { get; set; }

        [JsonPropertyName("art")]
        public string Art { get; set; }

        [JsonPropertyName("banner")]
        public object Banner { get; set; }

        [JsonPropertyName("duration")]
        public int Duration { get; set; }

        /// <summary>
        /// In format YYYY-MM-DD, e.g: 2017-02-19
        /// </summary>
        [JsonPropertyName("originallyAvailableAt")]
        public string OriginallyAvailableAt { get; set; }

        [JsonPropertyName("leafCount")]
        public int LeafCount { get; set; }

        [JsonPropertyName("viewedLeafCount")]
        public int ViewedLeafCount { get; set; }

        [JsonPropertyName("childCount")]
        public int ChildCount { get; set; }

        [JsonPropertyName("addedAt")]
        public long AddedAt { get; set; }

        [JsonPropertyName("updatedAt")]
        public long UpdatedAt { get; set; }

        [JsonPropertyName("Genre")]
        public IList<PlexLibraryGenreDTO> Genre { get; set; }

        [JsonPropertyName("Role")]
        public IList<PlexLibraryRoleDTO> Role { get; set; }

        [JsonPropertyName("viewCount")]
        public int? ViewCount { get; set; }

        [JsonPropertyName("lastViewedAt")]
        public long? LastViewedAt { get; set; }

        [JsonPropertyName("theme")]
        public object Theme { get; set; }
    }

    public class PlexLibraryGenreDTO
    {

        [JsonPropertyName("tag")]
        public string Tag { get; set; }
    }

    public class PlexLibraryRoleDTO
    {

        [JsonPropertyName("tag")]
        public string Tag { get; set; }
    }

}
