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

    public List<PlexServerSettingsModel> Data { get; set; } = new();

    #endregion

    #region Public Methods

    public Result<PlexServerSettingsModel> AddServerToSettings(PlexServerSettingsModel plexServerSettings)
    {
        if (string.IsNullOrEmpty(plexServerSettings.MachineIdentifier))
            return ResultExtensions.IsEmpty(nameof(plexServerSettings.MachineIdentifier));

        var settings = GetPlexServerSettings(plexServerSettings.MachineIdentifier);
        if (settings is not null)
        {
            _log.Information("A Server setting with {MachineIdentifier} already exists, will update now", plexServerSettings.MachineIdentifier);
            SetServerSettings(plexServerSettings);
        }
        else
        {
            Data.Add(plexServerSettings);
            EmitModuleHasChanged(GetValues());
        }

        return Result.Ok(settings);
    }

    public override IServerSettings DefaultValues()
    {
        return new ServerSettings
        {
            Data = CreateServerSettingsFromDb(new List<PlexServer>()),
        };
    }

    public IObservable<int> GetDownloadSpeedLimitObservable(string machineIdentifier)
    {
        return ServerSettings(machineIdentifier).Select(x => x.DownloadSpeedLimit);
    }

    public PlexServerSettingsModel GetPlexServerSettings(string machineIdentifier)
    {
        return Data?.FirstOrDefault(x => x.MachineIdentifier == machineIdentifier);
    }

    public int GetDownloadSpeedLimit(string machineIdentifier)
    {
        return GetPlexServerSettings(machineIdentifier)?.DownloadSpeedLimit ?? 0;
    }

    public override IServerSettings GetValues()
    {
        return new ServerSettings
        {
            Data = Data,
        };
    }

    public IObservable<PlexServerSettingsModel> ServerSettings(string machineIdentifier)
    {
        return _serverSettingsUpdated.AsObservable().Where(x => x.MachineIdentifier == machineIdentifier);
    }

    public Result SetDownloadSpeedLimit(string machineIdentifier, int downloadSpeedLimit = 0)
    {
        var index = Data.FindIndex(x => x.MachineIdentifier == machineIdentifier);
        if (index > -1)
        {
            if (Data[index].DownloadSpeedLimit != downloadSpeedLimit)
            {
                Data[index].DownloadSpeedLimit = downloadSpeedLimit;
                EmitModuleHasChanged(GetValues());
                return Result.Ok();
            }
        }

        return Result.Fail($"PlexServer with machineIdentifier \"{machineIdentifier}\" has no entry in the {nameof(ServerSettings)}");
    }

    public void SetServerSettings(PlexServerSettingsModel plexServerSettings)
    {
        var index = Data.FindIndex(x => x.MachineIdentifier == plexServerSettings.MachineIdentifier);
        if (index > -1)
        {
            if (Data[index].DownloadSpeedLimit != plexServerSettings.DownloadSpeedLimit)
            {
                Data[index].DownloadSpeedLimit = plexServerSettings.DownloadSpeedLimit;
                _serverSettingsUpdated.OnNext(Data[index]);
            }
        }
        else
        {
            Data.Add(plexServerSettings);
            _serverSettingsUpdated.OnNext(Data.Last());
        }
    }

    public override IServerSettings Update(IServerSettings sourceSettings)
    {
        if (sourceSettings?.Data is null)
        {
            _log.Warning("Can not update settings module {Name} with source settings null", Name);
            return GetValues();
        }

        foreach (var plexServerSettingsModel in sourceSettings.Data)
            SetServerSettings(plexServerSettingsModel);

        return GetValues();
    }

    public void EnsureAllServersHaveASettingsEntry(List<PlexServer> plexServers)
    {
        Data = CreateServerSettingsFromDb(plexServers);
        EmitModuleHasChanged(GetValues());
    }

    #endregion

    #region Private Methods

    private List<PlexServerSettingsModel> CreateServerSettingsFromDb(List<PlexServer> plexServers)
    {
        var newList = new List<PlexServerSettingsModel>();
        foreach (var plexServer in plexServers)
        {
            var index = Data.FindIndex(x => x.MachineIdentifier == plexServer.MachineIdentifier);
            if (index == -1)
            {
                newList.Add(new PlexServerSettingsModel
                {
                    MachineIdentifier = plexServer.MachineIdentifier,
                    DownloadSpeedLimit = 0,
                    PlexServerName = plexServer.Name,
                });
            }
        }

        return newList.Concat(Data).ToList();
    }

    #endregion
}