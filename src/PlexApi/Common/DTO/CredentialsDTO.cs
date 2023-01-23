using System.Text.Json.Serialization;

namespace PlexRipper.PlexApi;

public class CredentialsDTO
{
    [JsonPropertyName("login")]
    public string Login { get; set; }

    [JsonPropertyName("password")]
    public string Password { get; set; }

    [JsonPropertyName("rememberMe")]
    public bool RememberMe { get; set; }

    [JsonPropertyName("verificationCode")]
    public string VerificationCode { get; set; }
}