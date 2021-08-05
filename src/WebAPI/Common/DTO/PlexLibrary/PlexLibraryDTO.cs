using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PlexRipper.Domain;
using PlexRipper.WebAPI.Common.DTO.FolderPath;

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

        [JsonProperty("syncedAt", Required = Required.Always)]
        public DateTime SyncedAt { get; set; }

        [JsonProperty("outdated", Required = Required.Always)]
        public bool Outdated { get; set; }

        [JsonProperty("uuid", Required = Required.Always)]
        public Guid Uuid { get; set; }

        [JsonProperty("mediaSize", Required = Required.Always)]
        public long MediaSize { get; set; }

        [JsonProperty("libraryLocationId", Required = Required.Always)]
        public int LibraryLocationId { get; set; }

        [JsonProperty("libraryLocationPath", Required = Required.Always)]
        public string LibraryLocationPath { get; set; }

        [JsonProperty("defaultDestination", Required = Required.Always)]
        public FolderPathDTO DefaultDestination { get; set; }

        [JsonProperty("defaultDestinationId", Required = Required.Always)]
        public int DefaultDestinationId { get; set; }

        [JsonProperty("plexServerId", Required = Required.Always)]
        public int PlexServerId { get; set; }

        [JsonProperty("count", Required = Required.Always)]
        public int Count { get; set; }

        [JsonProperty("seasonCount", Required = Required.Always)]
        public int SeasonCount { get; set; }

        [JsonProperty("episodeCount", Required = Required.Always)]
        public int EpisodeCount { get; set; }

        [JsonProperty("movies", Required = Required.Always)]
        public List<PlexMediaDTO> Movies { get; set; }

        [JsonProperty("tvShows", Required = Required.Always)]
        public List<PlexMediaDTO> TvShows { get; set; }

        [JsonProperty("downloadTasks", Required = Required.Always)]
        public List<DownloadTaskDTO> DownloadTasks { get; set; }
    }
}