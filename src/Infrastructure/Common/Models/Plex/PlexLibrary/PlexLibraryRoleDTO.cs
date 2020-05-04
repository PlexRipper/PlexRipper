using Newtonsoft.Json;

namespace PlexRipper.Infrastructure.Common.Models.Plex.PlexLibrary
{
    public class PlexLibraryRoleDTO
    {
        [JsonProperty("tag")]
        public string Tag { get; set; }
    }
}
