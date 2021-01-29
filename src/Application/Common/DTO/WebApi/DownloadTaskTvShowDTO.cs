using System.Collections.Generic;
using Newtonsoft.Json;
using PlexRipper.Domain;

namespace PlexRipper.Application.Common.DTO.WebApi
{
    public class DownloadTaskTvShowDTO : DownloadTaskMediaDTO
    {
        [JsonProperty("seasons", Required = Required.Always)]
        public List<DownloadTaskTvShowSeasonDTO> Seasons { get; set; } = new List<DownloadTaskTvShowSeasonDTO>();

        [JsonProperty("mediaType", Required = Required.Always)]
        public override PlexMediaType MediaType => PlexMediaType.TvShow;
    }
}