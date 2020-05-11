using Newtonsoft.Json;

namespace PlexRipper.Infrastructure.Common.DTO.PlexSignIn
{
    public class PlexUserRequestDTO
    {
        [JsonProperty("user")]
        public UserRequestDTO User { get; set; }
    }
}
