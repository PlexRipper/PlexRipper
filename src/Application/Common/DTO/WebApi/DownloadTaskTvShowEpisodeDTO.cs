using Newtonsoft.Json;
using PlexRipper.Domain;

namespace PlexRipper.Application.Common.DTO.WebApi
{
    public class DownloadTaskTvShowEpisodeDTO : DownloadTaskMediaDTO
    {
        [JsonProperty("id", Required = Required.Always)]
        public int Id { get; set; }

        [JsonProperty("status", Required = Required.Always)]
        public DownloadStatus Status { get; set; }

        [JsonProperty("mediaType", Required = Required.Always)]
        public override PlexMediaType MediaType => PlexMediaType.Episode;

        [JsonProperty("destinationPath", Required = Required.Always)]
        public string DestinationPath { get; set; }

        [JsonProperty("downloadPath", Required = Required.Always)]
        public string DownloadPath { get; set; }

        [JsonProperty("downloadUrl", Required = Required.Always)]
        public string DownloadUrl { get; set; }
    }
}