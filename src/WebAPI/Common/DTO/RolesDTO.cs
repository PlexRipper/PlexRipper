using Newtonsoft.Json;

namespace PlexRipper.WebAPI.Common.DTO
{
    public class RolesDTO
    {
        [JsonProperty("roles")]
        public List<string> Roles { get; set; }
    }
}