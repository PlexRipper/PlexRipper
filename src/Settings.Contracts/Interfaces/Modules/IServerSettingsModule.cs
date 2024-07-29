namespace Settings.Contracts;

public interface IServerSettingsModule : IBaseSettingsModule<IServerSettings>, IServerSettings
{
    int GetDownloadSpeedLimit(string machineIdentifier);

    void SetDownloadSpeedLimit(string machineIdentifier, int downloadSpeedLimit = 0);

    IObservable<int> GetDownloadSpeedLimitObservable(string machineIdentifier);

    void SetServerName(string machineIdentifier, string serverName = "");

    string GetServerNameAlias(string machineIdentifier);

    void SetServerHiddenState(string machineIdentifier, bool isHidden);
}
