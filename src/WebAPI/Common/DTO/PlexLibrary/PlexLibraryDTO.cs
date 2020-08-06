using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace PlexRipper.WebAPI.Common.DTO
{
    public sealed class PlexLibraryDTO
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("updatedAt")]
        public DateTime UpdatedAt { get; set; }

        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("scannedAt")]
        public DateTime ScannedAt { get; set; }

        [JsonProperty("contentChangedAt")]
        public DateTime ContentChangedAt { get; set; }

        [JsonProperty("uuid")]
        public Guid Uuid { get; set; }

        [JsonProperty("libraryLocationId")]
        public int LibraryLocationId { get; set; }

        [JsonProperty("libraryLocationPath")]
        public string LibraryLocationPath { get; set; }

        [JsonProperty("plexServerId")]
        public int PlexServerId { get; set; }

        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("movies")]
        public List<PlexMovieDTO> Movies { get; set; }

        [JsonProperty("tvShows")]
        public List<PlexTvShowDTO> TvShows { get; set; }

    }
}
