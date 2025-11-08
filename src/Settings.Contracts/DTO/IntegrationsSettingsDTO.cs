namespace Reaparr.Settings.Contracts;

public class IntegrationsSettingsDTO
{
    public required SonarrSettingsDTO Sonarr { get; init; }
    public required RadarrSettingsDTO Radarr { get; init; }
}

public class RadarrSettingsDTO
{
    public required string BaseUrl { get; init; }
    public required string ApiKey { get; init; }
}

public class SonarrSettingsDTO
{
    public required string BaseUrl { get; init; }
    public required string ApiKey { get; init; }
}