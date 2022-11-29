using PlexRipper.Application;
using PlexRipper.Domain.DownloadManager;

namespace PlexRipper.Settings.Models;

public class ServerSettings : IServerSettings
{
    public List<PlexServerSettingsModel> Data { get; set; }
}