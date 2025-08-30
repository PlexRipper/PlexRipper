namespace Reaparr.Settings.Contracts;

public record AuthenticationModule : BaseSettingsModule<AuthenticationModule>, IAuthenticationSettings
{
    private bool _resetCredentials;

    public required bool ResetCredentials
    {
        get => _resetCredentials;
        set => SetProperty(ref _resetCredentials, value);
    }

    /// <summary>
    /// When true, will reset the Reaparr app credentials and then set to false again.
    /// </summary>
    /// <returns></returns>
    public static AuthenticationModule Create() => new() { ResetCredentials = false };
}
