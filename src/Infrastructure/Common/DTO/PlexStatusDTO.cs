using Newtonsoft.Json;
using PlexRipper.Infrastructure.Common.DTO.PlexGetStatus;

namespace PlexRipper.Infrastructure.Common.DTO
{

    public class PlexStatusDTO
    {
        [JsonProperty("mediaContainer")]

        public MediaContainerDTO MediaContainer { get; set; }
    }
}
