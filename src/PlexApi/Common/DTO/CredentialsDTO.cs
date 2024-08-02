namespace PlexRipper.PlexApi;

public record CredentialsDTO
{
    public required string Login { get; init; }

    public required string Password { get; init; }

    public required bool RememberMe { get; init; }

    public string VerificationCode { get; set; }
}
