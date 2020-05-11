using Newtonsoft.Json;
using System;

namespace PlexRipper.WebAPI.Common.DTO
{
    public class AccountDTO
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

        [JsonProperty("isValidated")]
        public bool IsValidated { get; set; }

        [JsonProperty("validatedAt")]
        public DateTime ValidatedAt { get; set; }

        [JsonProperty("plexAccount")]
        public PlexAccountDTO PlexAccount { get; set; }

    }
}
