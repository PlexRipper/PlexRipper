namespace Settings.Contracts;

public interface IServerSettings
{
    List<PlexServerSettingsModel> Data { get; init; }
}
