namespace Reaparr.Settings.Contracts;

public record AuthenticationModule
    : BaseSettingsModule<AuthenticationModule>,
        IBaseSettingsModule<AuthenticationModule>,
        IAuthenticationSettings
{
    public required bool ResetCredentials
    {
        get;
        set => SetProperty(ref field, value);
    }

    public required HeaderAuthenticationSettings HeaderAuthentication
    {
        get;
        set => SetProperty(ref field, value);
    } = HeaderAuthenticationSettings.Create();

    /// <summary>
    /// When true, will reset the Reaparr app credentials and then set to false again.
    /// </summary>
    /// <returns></returns>
    public static AuthenticationModule Create() =>
        new() { ResetCredentials = false, HeaderAuthentication = HeaderAuthenticationSettings.Create() };
}
