using Newtonsoft.Json;

namespace PlexRipper.Infrastructure.Common.Models.Plex.PlexLibrary
{
    public class PlexLibraryDTO
    {
        [JsonProperty("MediaContainer")]
        public PlexLibraryMediaContainerDTO MediaContainer { get; set; }
    }
}
