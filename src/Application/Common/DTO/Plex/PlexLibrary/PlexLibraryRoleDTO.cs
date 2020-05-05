using Newtonsoft.Json;

namespace PlexRipper.Application.Common.DTO.Plex.PlexLibrary
{
    public class PlexLibraryRoleDTO
    {
        [JsonProperty("tag")]
        public string Tag { get; set; }
    }
}
