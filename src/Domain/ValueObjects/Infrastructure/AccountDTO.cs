using Newtonsoft.Json;
using System;

namespace PlexRipper.Domain.ValueObjects
{
    public class AccountDTO
    {
        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("isConfirmed")]
        public bool IsConfirmed { get; set; }

        [JsonProperty("confirmedAt")]
        public DateTime ConfirmedAt { get; set; }
    }
}
