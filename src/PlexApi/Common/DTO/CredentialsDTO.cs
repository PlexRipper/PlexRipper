using System.Text.Json.Serialization;

namespace PlexRipper.PlexApi.Common.DTO;

public class CredentialsDTO
{
    [JsonPropertyName("username")]
    public string Username { get; set; }

    [JsonPropertyName("password")]
    public string Password { get; set; }

    [JsonPropertyName("rememberMe")]
    public bool RememberMe { get; set; }

    [JsonPropertyName("verificationCode")]
    public string VerificationCode { get; set; }
}