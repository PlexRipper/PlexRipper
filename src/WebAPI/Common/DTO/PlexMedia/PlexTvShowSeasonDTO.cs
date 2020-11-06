using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PlexRipper.Domain;

namespace PlexRipper.WebAPI.Common.DTO
{
    public class PlexTvShowSeasonDTO
    {
        [JsonProperty("id", Required = Required.Always)]
        public int Id { get; set; }

        [JsonProperty("ratingKey", Required = Required.Always)]
        public int RatingKey { get; set; }

        [JsonProperty("key", Required = Required.Always)]
        public string Key { get; set; }

        [JsonProperty("guid", Required = Required.Always)]
        public string Guid { get; set; }

        [JsonProperty("title", Required = Required.Always)]
        public string Title { get; set; }

        [JsonProperty("summary", Required = Required.Always)]
        public string Summary { get; set; }

        [JsonProperty("index", Required = Required.Always)]
        public int Index { get; set; }

        [JsonProperty("type", Required = Required.Always)]
        public PlexMediaType Type { get; set; }

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

        [JsonProperty("originallyAvailableAt", Required = Required.Always)]
        public DateTime OriginallyAvailableAt { get; set; }

        [JsonProperty("tvShowId", Required = Required.Always)]
        public int TvShowId { get; set; }

        [JsonProperty("episodes", Required = Required.Always)]
        public List<PlexTvShowEpisodeDTO> Episodes { get; set; }

        [JsonProperty("plexLibraryId", Required = Required.Always)]
        public int PlexLibraryId { get; set; }

    }
}
