using Newtonsoft.Json;
using PlexRipper.Domain.Extensions.Converters;
using System;
using System.Collections.Generic;

namespace PlexRipper.Infrastructure.Common.DTO.PlexLibraryMedia
{
    /// <summary>
    /// This DTO is used within PlexApi => GetLibraryMediaAsync. It returns the Metadata for all media inside a PlexLibrary
    /// </summary>
    public class PlexLibraryMediaDTO
    {
        [JsonProperty("MediaContainer")]
        public PlexLibraryMediaContainerDTO MediaContainer { get; set; }
    }

    public class PlexLibraryMediaContainerDTO
    {

        [JsonProperty("size")]
        public int Size { get; set; }

        [JsonProperty("allowSync")]
        public bool AllowSync { get; set; }

        [JsonProperty("art")]
        public string Art { get; set; }

        [JsonProperty("identifier")]
        public string Identifier { get; set; }

        [JsonProperty("librarySectionID")]
        public int LibrarySectionID { get; set; }

        [JsonProperty("librarySectionTitle")]
        public string LibrarySectionTitle { get; set; }

        [JsonProperty("librarySectionUUID")]
        public string LibrarySectionUUID { get; set; }

        [JsonProperty("mediaTagPrefix")]
        public string MediaTagPrefix { get; set; }

        [JsonProperty("mediaTagVersion")]
        public int MediaTagVersion { get; set; }

        [JsonProperty("nocache")]
        public bool Nocache { get; set; }

        [JsonProperty("thumb")]
        public string Thumb { get; set; }

        [JsonProperty("title1")]
        public string Title1 { get; set; }

        [JsonProperty("title2")]
        public string Title2 { get; set; }

        [JsonProperty("viewGroup")]
        public string ViewGroup { get; set; }

        [JsonProperty("viewMode")]
        public int ViewMode { get; set; }

        [JsonProperty("Metadata")]
        public IList<PlexLibraryMetaDataDTO> Metadata { get; set; }

    }

    /// <summary>
    /// The media container for the media metadata, this is the same for movies, tv shows and music
    /// </summary>
    public class PlexLibraryMetaDataDTO
    {

        [JsonProperty("ratingKey")]
        public string RatingKey { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("guid")]
        public string Guid { get; set; }

        [JsonProperty("studio")]
        public string Studio { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("contentRating")]
        public string ContentRating { get; set; }

        [JsonProperty("summary")]
        public string Summary { get; set; }

        [JsonProperty("index")]
        public int Index { get; set; }

        [JsonProperty("rating")]
        public double Rating { get; set; }

        [JsonProperty("year")]
        public int Year { get; set; }

        [JsonProperty("thumb")]
        public string Thumb { get; set; }

        [JsonProperty("art")]
        public string Art { get; set; }

        [JsonProperty("banner")]
        public object Banner { get; set; }

        [JsonProperty("duration")]
        public int Duration { get; set; }

        /// <summary>
        /// In format YYYY-MM-DD, e.g: 2017-02-19
        /// </summary>
        [JsonProperty("originallyAvailableAt")]
        public string OriginallyAvailableAt { get; set; }

        [JsonProperty("leafCount")]
        public int LeafCount { get; set; }

        [JsonProperty("viewedLeafCount")]
        public int ViewedLeafCount { get; set; }

        [JsonProperty("childCount")]
        public int ChildCount { get; set; }

        [JsonProperty("addedAt")]
        [JsonConverter(typeof(SafeUnixDateTimeConverter))]
        public DateTime AddedAt { get; set; }

        [JsonProperty("updatedAt")]
        [JsonConverter(typeof(SafeUnixDateTimeConverter))]
        public DateTime UpdatedAt { get; set; }

        [JsonProperty("Genre")]
        public IList<PlexLibraryGenreDTO> Genre { get; set; }

        [JsonProperty("Role")]
        public IList<PlexLibraryRoleDTO> Role { get; set; }

        [JsonProperty("viewCount")]
        public int? ViewCount { get; set; }

        [JsonProperty("lastViewedAt")]
        [JsonConverter(typeof(SafeUnixDateTimeConverter))]
        public DateTime? LastViewedAt { get; set; }

        [JsonProperty("theme")]
        public object Theme { get; set; }
    }

    public class PlexLibraryGenreDTO
    {

        [JsonProperty("tag")]
        public string Tag { get; set; }
    }

    public class PlexLibraryRoleDTO
    {

        [JsonProperty("tag")]
        public string Tag { get; set; }
    }

}
