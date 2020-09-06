using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using PlexRipper.Domain;

namespace PlexRipper.WebAPI.Common.DTO
{
    public sealed class PlexLibraryDTO
    {
        [JsonProperty("id", Required = Required.DisallowNull)]
        public int Id { get; set; }

        [JsonProperty("key", Required = Required.DisallowNull)]
        public string Key { get; set; }

        [JsonProperty("title", Required = Required.DisallowNull)]
        public string Title { get; set; }

        [JsonProperty("type", Required = Required.Always)]
        public PlexMediaType Type { get; set; }

        [JsonProperty("updatedAt", Required = Required.DisallowNull)]
        public DateTime UpdatedAt { get; set; }

        [JsonProperty("createdAt", Required = Required.DisallowNull)]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("scannedAt", Required = Required.DisallowNull)]
        public DateTime ScannedAt { get; set; }

        [JsonProperty("contentChangedAt", Required = Required.DisallowNull)]
        public DateTime ContentChangedAt { get; set; }

        [JsonProperty("uuid", Required = Required.DisallowNull)]
        public Guid Uuid { get; set; }

        [JsonProperty("libraryLocationId", Required = Required.DisallowNull)]
        public int LibraryLocationId { get; set; }

        [JsonProperty("libraryLocationPath", Required = Required.DisallowNull)]
        public string LibraryLocationPath { get; set; }

        [JsonProperty("plexServerId", Required = Required.DisallowNull)]
        public int PlexServerId { get; set; }

        [JsonProperty("count", Required = Required.DisallowNull)]
        public int Count { get; set; }

        [JsonProperty("movies", Required = Required.Always)]
        public List<PlexMovieDTO> Movies { get; set; }

        [JsonProperty("tvShows", Required = Required.Always)]
        public List<PlexTvShowDTO> TvShows { get; set; }

        [JsonProperty("downloadTasks", Required = Required.Always)]
        public List<DownloadTaskDTO> DownloadTasks { get; set; }
    }
}
