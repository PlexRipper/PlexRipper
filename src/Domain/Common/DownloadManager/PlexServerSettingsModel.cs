namespace PlexRipper.Domain.DownloadManager;

public record PlexServerSettingsModel
{
    public required string PlexServerName { get; set; } = string.Empty;

    public required string MachineIdentifier { get; init; }

    public required int DownloadSpeedLimit { get; set; }

    public required bool Hidden { get; set; }
}
