using PlexRipper.Domain.DownloadManager;

namespace Settings.Contracts;

public interface IServerSettings
{
    List<PlexServerSettingsModel> Data { get; init; }
}
