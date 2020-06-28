using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PlexRipper.PlexApi.Common.DTO
{
    public class RolesDTO
    {
        [JsonPropertyName("roles")]
        public List<string> Roles { get; set; }
    }
}
