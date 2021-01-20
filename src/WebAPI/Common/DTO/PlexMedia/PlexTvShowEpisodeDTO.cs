using System;
using Newtonsoft.Json;
using PlexRipper.Domain;

namespace PlexRipper.WebAPI.Common.DTO
{
    public class PlexTvShowEpisodeDTO
    {
        [JsonProperty("id", Required = Required.Always)]
        public int Id { get; set; }

        [JsonProperty("key", Required = Required.Always)]
        public int Key { get; set; }

        [JsonProperty("title", Required = Required.Always)]
        public string Title { get; set; }

        [JsonProperty("summary", Required = Required.Always)]
        public string Summary { get; set; }

        [JsonProperty("index", Required = Required.Always)]
        public int Index { get; set; }

        [JsonProperty("size", Required = Required.Always)]
        public long Size { get; set; }

        [JsonProperty("type", Required = Required.Always)]
        public PlexMediaType Type { get; set; }

        [JsonProperty("childCount", Required = Required.Always)]
        public int ChildCount { get; set; }

        [JsonProperty("addedAt", Required = Required.Always)]
        public DateTime AddedAt { get; set; }

        [JsonProperty("updatedAt", Required = Required.Always)]
        public DateTime UpdatedAt { get; set; }

        [JsonProperty("originallyAvailableAt", Required = Required.Always)]
        public DateTime? OriginallyAvailableAt { get; set; }

        [JsonProperty("tvShowSeasonId", Required = Required.Always)]
        public int TvShowSeasonId { get; set; }

        [JsonProperty("plexLibraryId", Required = Required.Always)]
        public int PlexLibraryId { get; set; }
    }
}