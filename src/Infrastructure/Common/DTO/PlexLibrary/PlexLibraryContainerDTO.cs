using Newtonsoft.Json;

namespace PlexRipper.Infrastructure.Common.DTO.PlexLibrary
{
    public class PlexLibraryContainerDTO
    {
        [JsonProperty("Mediacontainer")]
        public PlexMediaContainerDTO MediaContainer { get; set; }
    }
}
