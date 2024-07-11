namespace PlexRipper.Domain.DownloadManager;

public record PlexServerSettingsModel
{
    public required string PlexServerName { get; set; }

    public required string MachineIdentifier { get; set; }

    public required int DownloadSpeedLimit { get; set; }
}
