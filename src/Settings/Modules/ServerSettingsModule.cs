using System.Reactive.Linq;
using System.Reactive.Subjects;
using PlexRipper.Domain.DownloadManager;
using PlexRipper.Settings.Models;
using Settings.Contracts;

namespace PlexRipper.Settings.Modules;

public class ServerSettingsModule : BaseSettingsModule<IServerSettings>, IServerSettingsModule
{
    #region Fields

    private readonly Subject<PlexServerSettingsModel> _serverSettingsUpdated = new();

    #endregion

    #region Properties

    public override string Name => "ServerSettings";

    public List<PlexServerSettingsModel> Data { get; init; } = [];

    #endregion

    #region Public Methods

    public override IServerSettings DefaultValues() => new ServerSettings { Data = [] };

    public IObservable<int> GetDownloadSpeedLimitObservable(string machineIdentifier)
    {
        return _serverSettingsUpdated
            .Where(x => x.MachineIdentifier == machineIdentifier)
            .Select(x => x.DownloadSpeedLimit);
    }

    public int GetDownloadSpeedLimit(string machineIdentifier) =>
        FindOrAddServerSettingsModel(machineIdentifier)?.DownloadSpeedLimit ?? 0;

    public string GetServerNameAlias(string machineIdentifier) =>
        FindOrAddServerSettingsModel(machineIdentifier)?.PlexServerName ?? string.Empty;

    public override IServerSettings GetValues() => new ServerSettings { Data = Data };

    public void SetDownloadSpeedLimit(string machineIdentifier, int downloadSpeedLimit = 0)
    {
        var model = FindOrAddServerSettingsModel(machineIdentifier);
        if (model is null)
            return;

        if (model.DownloadSpeedLimit != downloadSpeedLimit)
        {
            model.DownloadSpeedLimit = downloadSpeedLimit;
            EmitModuleHasChanged(GetValues());
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
            EmitModuleHasChanged(GetValues());
        }
    }

    public override IServerSettings Update(IServerSettings sourceSettings)
    {
        if (!sourceSettings.Data.Any())
            return GetValues();

        foreach (var plexServerSettingsModel in sourceSettings.Data)
        {
            var model = FindOrAddServerSettingsModel(plexServerSettingsModel.MachineIdentifier);
            if (model is null)
                continue;

            if (model.DownloadSpeedLimit != plexServerSettingsModel.DownloadSpeedLimit)
                model.DownloadSpeedLimit = plexServerSettingsModel.DownloadSpeedLimit;

            if (model.PlexServerName != plexServerSettingsModel.PlexServerName)
                model.PlexServerName = plexServerSettingsModel.PlexServerName;
        }

        EmitModuleHasChanged(GetValues());

        return GetValues();
    }

    #endregion

    #region Private Methods

    private PlexServerSettingsModel? FindOrAddServerSettingsModel(string machineIdentifier)
    {
        if (machineIdentifier == string.Empty)
            return null;

        var index = Data.FindIndex(x => x.MachineIdentifier == machineIdentifier);
        if (index > -1)
        {
            _log.Information(
                "Could not create a new ServerSettingModel for {MachineIdentifier} because it already exists",
                machineIdentifier
            );
            return Data[index];
        }

        Data.Add(
            new PlexServerSettingsModel
            {
                MachineIdentifier = machineIdentifier,
                DownloadSpeedLimit = 0,
                PlexServerName = string.Empty,
            }
        );

        EmitModuleHasChanged(GetValues());
        return Data.Last();
    }

    protected override void EmitModuleHasChanged(IServerSettings module)
    {
        base.EmitModuleHasChanged(module);
        foreach (var model in module.Data)
            _serverSettingsUpdated.OnNext(model);
    }

    #endregion
}
