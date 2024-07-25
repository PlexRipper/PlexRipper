using PlexRipper.Domain.DownloadManager;

namespace Settings.Contracts;

public class ServerSettingsDTO : IServerSettings
{
    public required List<PlexServerSettingsModel> Data { get; init; }
}
