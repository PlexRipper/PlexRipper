using PlexRipper.Domain.DownloadManager;
using Settings.Contracts;

namespace PlexRipper.Settings.Models;

public class ServerSettings : IServerSettings
{
    public List<PlexServerSettingsModel> Data { get; set; }
}