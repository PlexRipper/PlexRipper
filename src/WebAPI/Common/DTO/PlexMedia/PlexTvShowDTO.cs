using System.Collections.Generic;
using Newtonsoft.Json;

namespace PlexRipper.WebAPI.Common.DTO
{
    public class PlexTvShowDTO : PlexMediaDTO
    {
        [JsonProperty("seasons", Required = Required.Always)]
        public List<PlexTvShowSeasonDTO> Seasons { get; set; }
    }
}