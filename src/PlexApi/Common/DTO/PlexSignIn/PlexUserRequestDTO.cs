using Newtonsoft.Json;

namespace PlexRipper.PlexApi.Common.DTO.PlexSignIn
{
    public class PlexUserRequestDTO
    {
        [JsonProperty("user")]
        public UserRequestDTO User { get; set; }
    }
}
