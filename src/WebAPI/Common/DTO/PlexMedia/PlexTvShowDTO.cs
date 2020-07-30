using Newtonsoft.Json;
using PlexRipper.WebAPI.Common.DTO.PlexMedia;
using System.Collections.Generic;

namespace PlexRipper.WebAPI.Common.DTO
{
    public class PlexTvShowDTO : PlexMediaDTO
    {
        [JsonProperty("seasons")]
        public List<PlexTvShowSeasonDTO> Seasons { get; set; }
    }

}
