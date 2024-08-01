using PlexRipper.Domain.DownloadManager;
using Settings.Contracts;

namespace PlexRipper.Settings;

public class ServerSettings : IServerSettings
{
    public List<PlexServerSettingsModel> Data { get; init; } = [];
}
