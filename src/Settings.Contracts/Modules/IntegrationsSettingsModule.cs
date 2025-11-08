namespace Reaparr.Settings.Contracts;

public record IntegrationsSettingsModule : BaseSettingsModule<IntegrationsSettingsModule>
{
    private SonarrSettings _sonarr = SonarrSettings.Create();
    private RadarrSettings _radarr = RadarrSettings.Create();

    public static IntegrationsSettingsModule Create() =>
        new() { Sonarr = SonarrSettings.Create(), Radarr = RadarrSettings.Create() };

    public required SonarrSettings Sonarr
    {
        get => _sonarr;
        set => SetProperty(ref _sonarr, value);
    }

    public required RadarrSettings Radarr
    {
        get => _radarr;
        set => SetProperty(ref _radarr, value);
    }
}
