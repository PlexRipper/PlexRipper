using Newtonsoft.Json;

namespace PlexRipper.Infrastructure.Common.DTO.PlexSignIn
{
    public class UserRequestDTO
    {
        [JsonProperty("login")]
        public string Login { get; set; }
        [JsonProperty("password")]
        public string Password { get; set; }
    }
}
