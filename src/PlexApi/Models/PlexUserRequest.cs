using System.Text.Json.Serialization;

namespace PlexRipper.PlexApi.Models
{
    public class UserRequest
    {
        [JsonPropertyName("login")]
        public string Login { get; set; }

        [JsonPropertyName("password")]
        public string Password { get; set; }

        [JsonPropertyName("verificationCode")]
        public int VerificationCode { get; set; }

        [JsonPropertyName("rememberMe")]
        public string RememberMe { get; set; }
    }
}