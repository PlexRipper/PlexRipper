using Newtonsoft.Json;

namespace PlexRipper.Application.Common.DTO.Plex.PlexLibrary
{
    public class PlexLibraryDTO
    {
        [JsonProperty("MediaContainer")]
        public PlexLibraryMediaContainerDTO MediaContainer { get; set; }
    }
}
