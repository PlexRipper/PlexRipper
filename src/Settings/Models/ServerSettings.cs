using PlexRipper.Domain.DownloadManager;
using Settings.Contracts;

namespace PlexRipper.Settings;

public record ServerSettings : IServerSettings
{
    public List<PlexServerSettingsModel> Data { get; init; } = [];
}
