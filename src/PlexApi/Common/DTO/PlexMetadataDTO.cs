using PlexRipper.PlexApi.Common.DTO.PlexGetStatus;
using System.Text.Json.Serialization;

namespace PlexRipper.PlexApi.Common.DTO
{
    public class PlexMetadataDTO
    {
        [JsonPropertyName("MediaContainer")]
        public MediaContainerDTO MediaContainerDto { get; set; }
    }
}
