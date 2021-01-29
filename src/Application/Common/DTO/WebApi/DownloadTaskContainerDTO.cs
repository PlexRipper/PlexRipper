using System.Collections.Generic;
using Newtonsoft.Json;

namespace PlexRipper.Application.Common.DTO.WebApi
{
    public class DownloadTaskContainerDTO
    {
        [JsonProperty("tvShows", Required = Required.Always)]
        public List<DownloadTaskTvShowDTO> TvShows { get; set; } = new List<DownloadTaskTvShowDTO>();
    }
}