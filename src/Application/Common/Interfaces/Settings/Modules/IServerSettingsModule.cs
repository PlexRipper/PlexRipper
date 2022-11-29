using PlexRipper.Domain.DownloadManager;

namespace PlexRipper.Application;

public interface IServerSettingsModule : IBaseSettingsModule<IServerSettings>, IServerSettings
{
    IObservable<PlexServerSettingsModel> ServerSettings(string machineIdentifier);

    void SetServerSettings(PlexServerSettingsModel plexServerSettings);

    PlexServerSettingsModel GetPlexServerSettings(string machineIdentifier);

    int GetDownloadSpeedLimit(string machineIdentifier);

    Result SetDownloadSpeedLimit(string machineIdentifier, int downloadSpeedLimit = 0);

    Result<PlexServerSettingsModel> AddServerToSettings(PlexServerSettingsModel plexServerSettings);

    IObservable<int> GetDownloadSpeedLimitObservable(string machineIdentifier);

    void EnsureAllServersHaveASettingsEntry(List<PlexServer> plexServers);
}