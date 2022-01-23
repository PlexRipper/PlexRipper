using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PlexRipper.BaseTests.DTO
{
    public class TestCredentialsDTO
    {
        [JsonPropertyName("credentials")]
        public List<TestAccountDTO> Credentials { get; set; }
    }

    public class TestAccountDTO
    {
        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("password")]
        public string Password { get; set; }
    }
}