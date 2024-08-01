using System.Reactive.Linq;

namespace Settings.Contracts;

public record ServerSettingsModel : BaseSettingsModel<ServerSettingsModel>, IServerSettings
{
    public List<PlexServerSettingsModel> Data { get; init; } = [];

    #region Public Methods

    public IObservable<int> GetDownloadSpeedLimitObservable(string machineIdentifier)
    {
        return HasChanged
            .SelectMany(x => x.Data)
            .Where(x => x.MachineIdentifier == machineIdentifier)
            .Select(x => x.DownloadSpeedLimit);
    }

    public int GetDownloadSpeedLimit(string machineIdentifier) =>
        FindOrAddServerSettingsModel(machineIdentifier)?.DownloadSpeedLimit ?? 0;

    public string GetServerNameAlias(string machineIdentifier) =>
        FindOrAddServerSettingsModel(machineIdentifier)?.PlexServerName ?? string.Empty;

    public void SetDownloadSpeedLimit(string machineIdentifier, int downloadSpeedLimit = 0)
    {
        var model = FindOrAddServerSettingsModel(machineIdentifier);
        if (model is null)
            return;

        if (model.DownloadSpeedLimit != downloadSpeedLimit)
            model.DownloadSpeedLimit = downloadSpeedLimit;
    }

    public void SetServerName(string machineIdentifier, string serverName = "")
    {
        var model = FindOrAddServerSettingsModel(machineIdentifier);
        if (model is null)
            return;

        model.PlexServerName = serverName;
    }

    public void SetServerHiddenState(string machineIdentifier, bool isHidden)
    {
        var model = FindOrAddServerSettingsModel(machineIdentifier);
        if (model is null)
            return;

        model.Hidden = isHidden;
    }

    #endregion

    #region Private Methods

    private PlexServerSettingsModel? FindOrAddServerSettingsModel(string machineIdentifier)
    {
        if (machineIdentifier == string.Empty)
            return null;

        var serverSettingsModel = Data.FirstOrDefault(x => x.MachineIdentifier == machineIdentifier);
        if (serverSettingsModel is not null)
            return serverSettingsModel;

        Data.Add(new PlexServerSettingsModel() { MachineIdentifier = machineIdentifier });

        OnPropertyChanged(nameof(Data));

        return Data.Last();
    }

    #endregion
}
