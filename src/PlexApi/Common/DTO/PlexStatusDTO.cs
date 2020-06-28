using PlexRipper.PlexApi.Common.DTO.PlexGetStatus;
using System.Text.Json.Serialization;

namespace PlexRipper.PlexApi.Common.DTO
{

    public class PlexStatusDTO
    {
        [JsonPropertyName("mediaContainer")]

        public MediaContainerDTO MediaContainer { get; set; }
    }
}
