using System.Text.Json.Serialization;

namespace PlexRipper.PlexApi.Common.DTO.PlexSignIn
{
    public class PlexUserRequestDTO
    {
        [JsonPropertyName("user")]
        public UserRequestDTO User { get; set; }
    }
}
