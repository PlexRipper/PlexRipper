using Newtonsoft.Json;
using System.Collections.Generic;

namespace PlexRipper.Application.Common.DTO.Plex
{
    public class RolesDTO
    {
        [JsonProperty("roles")]
        public List<string> Roles { get; set; }
    }
}
