using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.Json;
using FluentResults;
using Logging;
using PlexRipper.Application;
using PlexRipper.Domain.DownloadManager;
using PlexRipper.Settings.Models;

namespace PlexRipper.Settings.Modules
{
    public class ServerSettingsModule : BaseSettingsModule<IServerSettings>, IServerSettingsModule
    {
        #region Fields

        private readonly Subject<PlexServerSettingsModel> _serverSettingsUpdated = new();

        #endregion

        #region Properties

        public List<PlexServerSettingsModel> Data { get; set; } = new();

        public override string Name => "ServerSettings";

        public override IServerSettings DefaultValues => new ServerSettings
        {
            Data = new List<PlexServerSettingsModel>(),
        };

        #endregion

        #region Public Methods

        public PlexServerSettingsModel GetPlexServerSettings(string machineIdentifier)
        {
            return Data?.FirstOrDefault(x => x.MachineIdentifier == machineIdentifier);
        }

        public PlexServerSettingsModel GetPlexServerSettings(int plexServerId)
        {
            return Data?.FirstOrDefault(x => x.PlexServerId == plexServerId);
        }

        public int GetDownloadSpeedLimit(int plexServerId)
        {
            return GetPlexServerSettings(plexServerId)?.DownloadSpeedLimit ?? 0;
        }

        public void Reset()
        {
            Update(new ServerSettingsModule());
        }

        public IObservable<PlexServerSettingsModel> ServerSettings(int plexServerId)
        {
            return _serverSettingsUpdated.AsObservable().Where(x => x.PlexServerId == plexServerId);
        }

        public IObservable<int> GetDownloadSpeedLimitObservable(int plexServerId)
        {
            return ServerSettings(plexServerId).Select(x => x.DownloadSpeedLimit);
        }

        public Result SetFromJson(JsonElement settingsJsonElement)
        {
            var jsonSettings = GetJsonSettingsModule(settingsJsonElement);
            if (jsonSettings.IsFailed)
            {
                Reset();
                return jsonSettings;
            }

            var serverSettings = jsonSettings.Value;

            try
            {
                Data = JsonSerializer.Deserialize<List<PlexServerSettingsModel>>(serverSettings.GetRawText());
            }
            catch (Exception e)
            {
                Log.Error(e);
                Reset();
            }

            return Result.Ok();
        }

        public override IServerSettings GetValues()
        {
            return new ServerSettings
            {
                Data = Data,
            };
        }

        public Result<PlexServerSettingsModel> AddServerToSettings(PlexServerSettingsModel plexServerSettings)
        {
            if (string.IsNullOrEmpty(plexServerSettings.MachineIdentifier))
                return Result.Fail(
                    $"Could not add server to settings because the {nameof(PlexServerSettingsModel.MachineIdentifier)} was invalid: {plexServerSettings.MachineIdentifier}");

            if (plexServerSettings.PlexServerId <= 0)
                return Result.Fail(
                    $"Could not add server to settings because the {nameof(PlexServerSettingsModel.PlexServerId)} was invalid: {plexServerSettings.PlexServerId}");

            var settings = GetPlexServerSettings(plexServerSettings.MachineIdentifier);
            if (settings is not null)
            {
                Log.Information($"A Server setting with {plexServerSettings.MachineIdentifier} already exists, will update now.");
                SetServerSettings(plexServerSettings);
            }
            else
            {
                Data.Add(plexServerSettings);
                EmitModuleHasChanged(GetValues());
            }

            return Result.Ok(settings);
        }

        public void SetServerSettings(PlexServerSettingsModel plexServerSettings)
        {
            var index = Data.FindIndex(x => x.PlexServerId == plexServerSettings.PlexServerId &&
                                            x.MachineIdentifier == plexServerSettings.MachineIdentifier);
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

        public Result SetDownloadSpeedLimit(int plexServerId, int downloadSpeedLimit = 0)
        {
            var index = Data.FindIndex(x => x.PlexServerId == plexServerId);
            if (index > -1)
            {
                if (Data[index].DownloadSpeedLimit != downloadSpeedLimit)
                {
                    Data[index].DownloadSpeedLimit = downloadSpeedLimit;
                    EmitModuleHasChanged(GetValues());
                    return Result.Ok();
                }
            }

            return Result.Fail($"PlexServerId {plexServerId} has no entry in the {nameof(ServerSettings)}");
        }

        public Result Update(IServerSettingsModule sourceSettings)
        {
            Data = sourceSettings.Data;
            return Result.Ok();
        }

        #endregion
    }
}