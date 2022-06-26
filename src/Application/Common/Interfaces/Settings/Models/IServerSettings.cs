using PlexRipper.Domain.DownloadManager;

namespace PlexRipper.Application;

public interface IServerSettings
{
    List<PlexServerSettingsModel> Data { get; set; }
}