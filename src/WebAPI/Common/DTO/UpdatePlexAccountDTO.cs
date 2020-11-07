using Newtonsoft.Json;

namespace PlexRipper.WebAPI.Common.DTO
{
    public class UpdatePlexAccountDTO
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("isEnabled")]
        public bool IsEnabled { get; set; }

        [JsonProperty("isMain")]
        public bool IsMain { get; set; }
    }
}