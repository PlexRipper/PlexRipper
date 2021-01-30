using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PlexRipper.Application.Common.DTO.WebApi;
using PlexRipper.Domain;

namespace PlexRipper.WebAPI.Common.DTO
{
    public sealed class PlexLibraryDTO
    {
        [JsonProperty("id", Required = Required.Always)]
        public int Id { get; set; }

        [JsonProperty("key", Required = Required.Always)]
        public string Key { get; set; }

        [JsonProperty("title", Required = Required.Always)]
        public string Title { get; set; }

        [JsonProperty("type", Required = Required.Always)]
        public PlexMediaType Type { get; set; }

        [JsonProperty("updatedAt", Required = Required.Always)]
        public DateTime UpdatedAt { get; set; }

        [JsonProperty("createdAt", Required = Required.Always)]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("scannedAt", Required = Required.Always)]
        public DateTime ScannedAt { get; set; }

        [JsonProperty("contentChangedAt", Required = Required.Always)]
        public DateTime ContentChangedAt { get; set; }

        [JsonProperty("uuid", Required = Required.Always)]
        public Guid Uuid { get; set; }

        [JsonProperty("mediaSize", Required = Required.Always)]
        public long MediaSize { get; set; }

        [JsonProperty("libraryLocationId", Required = Required.Always)]
        public int LibraryLocationId { get; set; }

        [JsonProperty("libraryLocationPath", Required = Required.Always)]
        public string LibraryLocationPath { get; set; }

        [JsonProperty("plexServerId", Required = Required.Always)]
        public int PlexServerId { get; set; }

        [JsonProperty("count", Required = Required.Always)]
        public int Count { get; set; }

        [JsonProperty("seasonCount", Required = Required.Always)]
        public int SeasonCount { get; set; }

        [JsonProperty("episodeCount", Required = Required.Always)]
        public int EpisodeCount { get; set; }

        [JsonProperty("movies", Required = Required.Always)]
        public List<PlexMovieDTO> Movies { get; set; }

        [JsonProperty("tvShows", Required = Required.Always)]
        public List<PlexTvShowDTO> TvShows { get; set; }

        [JsonProperty("downloadTasks", Required = Required.Always)]
        public List<DownloadTaskDTO> DownloadTasks { get; set; }
    }
}