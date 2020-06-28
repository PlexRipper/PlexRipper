using System.Text.Json.Serialization;

namespace PlexRipper.PlexApi.Common.DTO.PlexSignIn
{
    public class UserRequestDTO
    {
        [JsonPropertyName("login")]
        public string Login { get; set; }
        [JsonPropertyName("password")]
        public string Password { get; set; }
    }
}
