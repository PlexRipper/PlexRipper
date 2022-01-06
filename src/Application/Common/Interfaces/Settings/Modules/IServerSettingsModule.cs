using System;
using FluentResults;
using PlexRipper.Domain.DownloadManager;

namespace PlexRipper.Application
{
    public interface IServerSettingsModule : IBaseSettingsModule<IServerSettingsModule, IServerSettings>, IServerSettings
    {
        IObservable<PlexServerSettingsModel> ServerSettings(int plexServerId);

        void SetServerSettings(PlexServerSettingsModel plexServerSettings);

        PlexServerSettingsModel GetPlexServerSettings(string machineIdentifier);

        PlexServerSettingsModel GetPlexServerSettings(int plexServerId);

        int GetDownloadSpeedLimit(int plexServerId);

        Result SetDownloadSpeedLimit(int plexServerId, int downloadSpeedLimit = 0);

        Result<PlexServerSettingsModel> AddServerToSettings(PlexServerSettingsModel plexServerSettings);
    }
}