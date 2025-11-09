namespace Reaparr.Settings.Contracts;

public class IntegrationsSettingsDTO
{
    public required string ReaparrApiKey { get; init; }

    public required string DownloadClientUsername { get; init; }

    public required string DownloadClientPassword { get; init; }

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
    public required string SonarrBaseUrl { get; init; }

    public required string SonarrApiKey { get; init; }
}
