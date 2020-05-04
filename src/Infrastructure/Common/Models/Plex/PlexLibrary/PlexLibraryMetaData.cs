using Newtonsoft.Json;
using System.Collections.Generic;

namespace PlexRipper.Infrastructure.Common.Models.Plex.PlexLibrary
{
    public class PlexLibraryMetaData
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
        public string Banner { get; set; }

        [JsonProperty("duration")]
        public int Duration { get; set; }

        [JsonProperty("originallyAvailableAt")]
        public string OriginallyAvailableAt { get; set; }

        [JsonProperty("leafCount")]
        public int LeafCount { get; set; }

        [JsonProperty("viewedLeafCount")]
        public int ViewedLeafCount { get; set; }

        [JsonProperty("childCount")]
        public int ChildCount { get; set; }

        [JsonProperty("addedAt")]
        public int AddedAt { get; set; }

        [JsonProperty("updatedAt")]
        public int UpdatedAt { get; set; }

        [JsonProperty("Genre")]
        public IList<PlexLibraryGenreDTO> Genre { get; set; }

        [JsonProperty("Role")]
        public IList<PlexLibraryRoleDTO> Role { get; set; }

        [JsonProperty("viewCount")]
        public int? ViewCount { get; set; }

        [JsonProperty("lastViewedAt")]
        public int? LastViewedAt { get; set; }

        [JsonProperty("theme")]
        public string Theme { get; set; }
    }
}
