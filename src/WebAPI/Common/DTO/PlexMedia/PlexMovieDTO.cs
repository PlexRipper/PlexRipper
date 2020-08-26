using System;
using Newtonsoft.Json;

namespace PlexRipper.WebAPI.Common.DTO
{
    public class PlexMovieDTO
    {
        [JsonProperty("id", Required = Required.Always)]
        public int Id { get; set; }

        [JsonProperty("ratingKey")]
        public int RatingKey { get; set; }

        [JsonProperty("key")]
        public object Key { get; set; }

        [JsonProperty("guid")]
        public object Guid { get; set; }

        [JsonProperty("studio")]
        public object Studio { get; set; }

        [JsonProperty("title")]
        public object Title { get; set; }

        [JsonProperty("contentRating")]
        public object ContentRating { get; set; }

        [JsonProperty("summary")]
        public object Summary { get; set; }

        [JsonProperty("index")]
        public int Index { get; set; }

        [JsonProperty("rating")]
        public double Rating { get; set; }

        [JsonProperty("year")]
        public int Year { get; set; }

        [JsonProperty("thumb")]
        public object Thumb { get; set; }

        [JsonProperty("art")]
        public object Art { get; set; }

        [JsonProperty("banner")]
        public object Banner { get; set; }

        [JsonProperty("duration")]
        public int Duration { get; set; }

        [JsonProperty("originallyAvailableAt")]
        public DateTime OriginallyAvailableAt { get; set; }

        [JsonProperty("leafCount")]
        public int LeafCount { get; set; }

        [JsonProperty("viewedLeafCount")]
        public int ViewedLeafCount { get; set; }

        [JsonProperty("childCount")]
        public int ChildCount { get; set; }

        [JsonProperty("addedAt")]
        public DateTime AddedAt { get; set; }

        [JsonProperty("updatedAt")]
        public DateTime UpdatedAt { get; set; }

        [JsonProperty("viewCount")]
        public object ViewCount { get; set; }

        [JsonProperty("lastViewedAt")]
        public object LastViewedAt { get; set; }

        [JsonProperty("theme")]
        public object Theme { get; set; }

        [JsonProperty("plexLibraryId", Required = Required.Always)]
        public int PlexLibraryId { get; set; }
    }
}