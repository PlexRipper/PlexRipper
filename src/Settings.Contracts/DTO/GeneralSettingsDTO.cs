namespace Settings.Contracts;

public class GeneralSettingsDTO : IGeneralSettings
{
    public required bool FirstTimeSetup { get; set; }

    public required int ActiveAccountId { get; set; }

    public required bool DisableAnimatedBackground { get; set; }

    public required bool HideMediaFromOfflineServers { get; set; }

    public required bool HideMediaFromOwnedServers { get; set; }

    public required bool UseLowQualityPosterImages { get; set; }

    public required bool HasAgreedToDisclaimer { get; set; }

    public required bool HasBeenInvitedToDiscord { get; set; }
}
