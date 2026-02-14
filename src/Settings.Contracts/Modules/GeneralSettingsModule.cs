namespace Reaparr.Settings.Contracts;

public record GeneralSettingsModule
    : BaseSettingsModule<GeneralSettingsModule>,
        IBaseSettingsModule<GeneralSettingsModule>,
        IGeneralSettings
{
    public static GeneralSettingsModule Create() =>
        new()
        {
            FirstTimeSetup = true,
            ActiveAccountId = 0,
            DisableAnimatedBackground = false,
            HideMediaFromOfflineServers = false,
            HideMediaFromOwnedServers = false,
            UseLowQualityPosterImages = false,
            HasBeenInvitedToDiscord = false,
        };

    public required bool FirstTimeSetup
    {
        get;
        set => SetProperty(ref field, value);
    } = true;

    public required int ActiveAccountId
    {
        get;
        set => SetProperty(ref field, value);
    }

    public required bool DisableAnimatedBackground
    {
        get;
        set => SetProperty(ref field, value);
    }

    public required bool HideMediaFromOfflineServers
    {
        get;
        set => SetProperty(ref field, value);
    }

    public required bool HideMediaFromOwnedServers
    {
        get;
        set => SetProperty(ref field, value);
    }

    public required bool UseLowQualityPosterImages
    {
        get;
        set => SetProperty(ref field, value);
    }

    public required bool HasBeenInvitedToDiscord
    {
        get;
        set => SetProperty(ref field, value);
    }

    public bool HasAgreedToDisclaimer
    {
        get;
        set => SetProperty(ref field, value);
    }
}
