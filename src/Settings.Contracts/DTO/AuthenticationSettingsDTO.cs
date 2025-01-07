namespace Settings.Contracts;

public record AuthenticationSettingsDTO
{
    public required string Username { get; set; }

    public required string Password { get; set; }
}
