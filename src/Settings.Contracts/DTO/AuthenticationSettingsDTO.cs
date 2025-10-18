namespace Reaparr.Settings.Contracts;

public record AuthenticationSettingsDTO
{
    public required bool ResetCredentials { get; set; }

    public required HeaderAuthenticationSettingsDTO HeaderAuthentication { get; set; }
}
