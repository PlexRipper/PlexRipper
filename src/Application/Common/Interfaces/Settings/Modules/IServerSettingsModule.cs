using System;
using FluentResults;
using PlexRipper.Domain.DownloadManager;

namespace PlexRipper.Application
{
    public interface IServerSettingsModule : IBaseSettingsModule<IServerSettings>, IServerSettings
    {
        IObservable<PlexServerSettingsModel> ServerSettings(int plexServerId);

        void SetServerSettings(PlexServerSettingsModel plexServerSettings);

        PlexServerSettingsModel GetPlexServerSettings(string machineIdentifier);

        PlexServerSettingsModel GetPlexServerSettings(int plexServerId);

        int GetDownloadSpeedLimit(int plexServerId);

        Result SetDownloadSpeedLimit(int plexServerId, int downloadSpeedLimit = 0);

        Result<PlexServerSettingsModel> AddServerToSettings(PlexServerSettingsModel plexServerSettings);

        IObservable<int> GetDownloadSpeedLimitObservable(int plexServerId);

        void EnsureAllServersHaveASettingsEntry();
    }
}