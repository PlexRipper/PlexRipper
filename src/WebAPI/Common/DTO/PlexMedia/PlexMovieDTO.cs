using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PlexRipper.WebAPI.Common.DTO.PlexMediaData;

namespace PlexRipper.WebAPI.Common.DTO
{
    public class PlexMovieDTO
    {
        [JsonProperty("id", Required = Required.Always)]
        public int Id { get; set; }

        [JsonProperty("key", Required = Required.Always)]
        public int Key { get; set; }

        [JsonProperty("guid", Required = Required.Always)]
        public object Guid { get; set; }

        [JsonProperty("studio", Required = Required.Always)]
        public string Studio { get; set; }

        [JsonProperty("title", Required = Required.Always)]
        public string Title { get; set; }

        [JsonProperty("size", Required = Required.Always)]
        public long Size { get; set; }

        [JsonProperty("contentRating", Required = Required.Always)]
        public string ContentRating { get; set; }

        [JsonProperty("summary", Required = Required.Always)]
        public string Summary { get; set; }

        [JsonProperty("index", Required = Required.Always)]
        public int Index { get; set; }

        [JsonProperty("rating", Required = Required.Always)]
        public double Rating { get; set; }

        [JsonProperty("year", Required = Required.Always)]
        public int Year { get; set; }

        [JsonProperty("thumb", Required = Required.Always)]
        public object Thumb { get; set; }

        [JsonProperty("art", Required = Required.Always)]
        public object Art { get; set; }

        [JsonProperty("banner", Required = Required.Always)]
        public object Banner { get; set; }

        [JsonProperty("duration", Required = Required.Always)]
        public int Duration { get; set; }

        [JsonProperty("originallyAvailableAt", Required = Required.Always)]
        public DateTime OriginallyAvailableAt { get; set; }

        [JsonProperty("leafCount", Required = Required.Always)]
        public int LeafCount { get; set; }

        [JsonProperty("viewedLeafCount", Required = Required.Always)]
        public int ViewedLeafCount { get; set; }

        [JsonProperty("childCount", Required = Required.Always)]
        public int ChildCount { get; set; }

        [JsonProperty("addedAt", Required = Required.Always)]
        public DateTime AddedAt { get; set; }

        [JsonProperty("updatedAt", Required = Required.Always)]
        public DateTime UpdatedAt { get; set; }

        [JsonProperty("viewCount", Required = Required.Always)]
        public object ViewCount { get; set; }

        [JsonProperty("theme", Required = Required.Always)]
        public object Theme { get; set; }

        [JsonProperty("plexLibraryId", Required = Required.Always)]
        public int PlexLibraryId { get; set; }

        [JsonProperty("plexMovieDatas", Required = Required.Always)]
        public List<PlexMovieDataDTO> PlexMovieDatas { get; set; }
    }
}