using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace PlexRipper.WebAPI.Common.DTO
{
    public class PlexTvShowSeasonDTO
    {
        [JsonProperty("id", Required = Required.Always)]
        public int Id { get; set; }

        [JsonProperty("ratingKey")]
        public int RatingKey { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("guid")]
        public string Guid { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("summary")]
        public string Summary { get; set; }

        [JsonProperty("index")]
        public int Index { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

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

        [JsonProperty("originallyAvailableAt")]
        public DateTime OriginallyAvailableAt { get; set; }

        [JsonProperty("tvShowId", Required = Required.Always)]
        public int TvShowId { get; set; }

        [JsonProperty("episodes")]
        public List<PlexTvShowEpisodeDTO> Episodes { get; set; }

    }
}
