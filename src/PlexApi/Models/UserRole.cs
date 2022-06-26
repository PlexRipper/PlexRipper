using System.Text.Json.Serialization;

namespace PlexRipper.PlexApi.Models
{
    public class UserRole
    {
        [JsonPropertyName("Roles")]
        public List<string> Roles { get; set; }
    }
}