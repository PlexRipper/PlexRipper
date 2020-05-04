using Newtonsoft.Json;

namespace PlexRipper.Application.Common.Models
{
    public class PlexContainer
    {
        [JsonProperty("Mediacontainer")]
        public MediaContainer MediaContainer { get; set; }
    }
}
