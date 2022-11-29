namespace PlexRipper.Domain.DownloadManager;

public class PlexServerSettingsModel
{
    public string PlexServerName { get; set; }

    public string MachineIdentifier { get; set; }

    public int DownloadSpeedLimit { get; set; }
}