using System.Reactive.Linq;

namespace Settings.Contracts;

public record PlexServerSettingsModule : BaseSettingsModule<PlexServerSettingsModule>, IServerSettingsModule
{
    public List<PlexServerSettingItemModule> Data { get; init; } = [];

    #region Public Methods

    public IObservable<int> GetDownloadSpeedLimitObservable(string machineIdentifier)
    {
        FindOrAddServerSettingsModel(machineIdentifier);

        return HasChanged
            .SelectMany(x => x.Data)
            .Where(x => x.MachineIdentifier == machineIdentifier)
            .Select(x => x.DownloadSpeedLimit)
            .DistinctUntilChanged();
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
        {
            model.DownloadSpeedLimit = downloadSpeedLimit;
            OnPropertyChanged(nameof(model.DownloadSpeedLimit));
        }
    }

    public void SetServerName(string machineIdentifier, string serverName = "")
    {
        var model = FindOrAddServerSettingsModel(machineIdentifier);
        if (model is null)
            return;

        if (model.PlexServerName != serverName)
        {
            model.PlexServerName = serverName;
            OnPropertyChanged(nameof(model.PlexServerName));
        }
    }

    public void SetServerHiddenState(string machineIdentifier, bool isHidden)
    {
        var model = FindOrAddServerSettingsModel(machineIdentifier);
        if (model is null)
            return;

        if (model.Hidden != isHidden)
        {
            model.Hidden = isHidden;
            OnPropertyChanged(nameof(model.Hidden));
        }
    }

    #endregion

    #region Private Methods

    private PlexServerSettingItemModule? FindOrAddServerSettingsModel(string machineIdentifier)
    {
        if (machineIdentifier == string.Empty)
            return null;

        var serverSettingsModel = Data.FirstOrDefault(x => x.MachineIdentifier == machineIdentifier);
        if (serverSettingsModel is not null)
            return serverSettingsModel;

        Data.Add(new PlexServerSettingItemModule() { MachineIdentifier = machineIdentifier });

        return Data.Last();
    }

    #endregion
}
