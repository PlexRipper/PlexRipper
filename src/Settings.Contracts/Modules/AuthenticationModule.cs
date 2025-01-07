namespace Settings.Contracts;

public record AuthenticationModule : BaseSettingsModule<AuthenticationModule>, IAuthenticationSettings
{
    public static AuthenticationModule Create() => new() { };
}
