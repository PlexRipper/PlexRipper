namespace PlexRipper.Domain.DownloadManager
{
    public class PlexServerSettingsModel
    {
        public int PlexServerId { get; set; }

        public string MachineIdentifier { get; set; }

        public int DownloadSpeedLimit { get; set; }
    }
}