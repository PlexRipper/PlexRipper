using Newtonsoft.Json;
using PlexRipper.PlexApi.Common.DTO.PlexGetStatus;

namespace PlexRipper.PlexApi.Common.DTO
{

    public class PlexStatusDTO
    {
        [JsonProperty("mediaContainer")]

        public MediaContainerDTO MediaContainer { get; set; }
    }
}
