using PlexRipper.Domain.DownloadManager;

namespace Settings.Contracts;

public class ServerSettingsDTO : IServerSettings
{
    public List<PlexServerSettingsModel> Data { get; set; }
}