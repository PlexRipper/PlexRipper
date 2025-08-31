namespace Reaparr.Settings.Contracts;

public interface IGeneralSettings
{
    bool FirstTimeSetup { get; set; }

    int ActiveAccountId { get; set; }

    bool DisableAnimatedBackground { get; set; }

    bool HideMediaFromOfflineServers { get; set; }

    bool HideMediaFromOwnedServers { get; set; }

    bool UseLowQualityPosterImages { get; set; }

    bool HasAgreedToDisclaimer { get; set; }
}
