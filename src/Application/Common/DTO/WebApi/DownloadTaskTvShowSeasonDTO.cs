using System.Collections.Generic;
using Newtonsoft.Json;
using PlexRipper.Domain;

namespace PlexRipper.Application.Common.DTO.WebApi
{
    public class DownloadTaskTvShowSeasonDTO : DownloadTaskMediaDTO
    {
        [JsonProperty("episodes", Required = Required.Always)]
        public List<DownloadTaskTvShowEpisodeDTO> Episodes { get; set; } = new List<DownloadTaskTvShowEpisodeDTO>();

        [JsonProperty("mediaType", Required = Required.Always)]
        public override PlexMediaType MediaType => PlexMediaType.Season;

    }
}