using System.Text.Json.Serialization;

namespace PlexRipper.PlexApi.Models
{
    public class PlexUserRequest
    {
        [JsonPropertyName("user")]
        public UserRequest User { get; set; }
    }

    public class UserRequest
    {
        [JsonPropertyName("login")]
        public string Login { get; set; }

        [JsonPropertyName("password")]
        public string Password { get; set; }
    }
}