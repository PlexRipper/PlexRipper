namespace Reaparr.Settings.Contracts;

public interface IServerSettings
{
    List<PlexServerSettingItemModule> Data { get; init; }
}
