using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PlexRipper.Domain;
using PlexRipper.WebAPI.Common.DTO.PlexMediaData;

namespace PlexRipper.WebAPI.Common.DTO
{
    public class PlexMediaDTO
    {
        [JsonProperty("id", Required = Required.Always)]
        public int Id { get; set; }

        [JsonProperty("key", Required = Required.Always)]
        public int Key { get; set; }

        /// <summary>
        /// Used specifically for the treeView display in the client
        /// </summary>
        [JsonProperty("treeKeyId", Required = Required.Always)]
        public string TreeKeyId { get; set; }

        [JsonProperty("title", Required = Required.Always)]
        public string Title { get; set; }

        [JsonProperty("year", Required = Required.Always)]
        public int Year { get; set; }

        [JsonProperty("duration", Required = Required.Always)]
        public int Duration { get; set; }

        [JsonProperty("mediaSize", Required = Required.Always)]
        public long MediaSize { get; set; }

        [JsonProperty("hasThumb", Required = Required.Always)]
        public bool HasThumb { get; set; }

        [JsonProperty("hasArt", Required = Required.Always)]
        public bool HasArt { get; set; }

        [JsonProperty("hasBanner", Required = Required.Always)]
        public bool HasBanner { get; set; }

        [JsonProperty("hasTheme", Required = Required.Always)]
        public bool HasTheme { get; set; }

        [JsonProperty("index", Required = Required.Always)]
        public int Index { get; set; }

        [JsonProperty("studio", Required = Required.Always)]
        public string Studio { get; set; }

        [JsonProperty("summary", Required = Required.Always)]
        public string Summary { get; set; }

        [JsonProperty("contentRating", Required = Required.Always)]
        public string ContentRating { get; set; }

        [JsonProperty("rating", Required = Required.Always)]
        public double Rating { get; set; }

        [JsonProperty("childCount", Required = Required.Always)]
        public int ChildCount { get; set; }

        [JsonProperty("addedAt", Required = Required.Always)]
        public DateTime AddedAt { get; set; }

        [JsonProperty("updatedAt", Required = Required.Always)]
        public DateTime UpdatedAt { get; set; }

        [JsonProperty("originallyAvailableAt", Required = Required.Always)]
        public DateTime? OriginallyAvailableAt { get; set; }

        [JsonProperty("tvShowId", Required = Required.Always)]
        public int TvShowId { get; set; }

        [JsonProperty("tvShowSeasonId", Required = Required.Always)]
        public int TvShowSeasonId { get; set; }

        [JsonProperty("plexLibraryId", Required = Required.Always)]
        public int PlexLibraryId { get; set; }

        [JsonProperty("plexServerId", Required = Required.Always)]
        public int PlexServerId { get; set; }

        [JsonProperty("type", Required = Required.Always)]
        public PlexMediaType Type { get; set; }

        [JsonProperty("mediaData", Required = Required.AllowNull)]
        public List<PlexMediaDataDTO> MediaData { get; set; }

        [JsonProperty("children", Required = Required.Always)]
        public List<PlexMediaDTO> Children { get; set; } = new List<PlexMediaDTO>();
    }
}