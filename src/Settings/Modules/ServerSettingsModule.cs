using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.Json;
using FluentResults;
using Logging;
using MediatR;
using PlexRipper.Application;
using PlexRipper.Domain.Config;
using PlexRipper.Domain.DownloadManager;
using PlexRipper.Settings.Models;

namespace PlexRipper.Settings.Modules
{
    public class ServerSettingsModule : BaseSettingsModule<IServerSettings>, IServerSettingsModule
    {
        #region Fields

        private readonly IMediator _mediator;

        private readonly Subject<PlexServerSettingsModel> _serverSettingsUpdated = new();

        #endregion

        #region Constructor

        public ServerSettingsModule() { }

        public ServerSettingsModule(IMediator mediator)
        {
            _mediator = mediator;
        }

        #endregion

        #region Properties

        public override string Name => "ServerSettings";

        public List<PlexServerSettingsModel> Data { get; set; } = new();

        #endregion

        #region Public Methods

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

        public override IServerSettings DefaultValues()
        {
            return new ServerSettings
            {
                Data = CreateServerSettingsFromDb(),
            };
        }

        public int GetDownloadSpeedLimit(int plexServerId)
        {
            return GetPlexServerSettings(plexServerId)?.DownloadSpeedLimit ?? 0;
        }

        public IObservable<int> GetDownloadSpeedLimitObservable(int plexServerId)
        {
            return ServerSettings(plexServerId).Select(x => x.DownloadSpeedLimit);
        }

        public PlexServerSettingsModel GetPlexServerSettings(string machineIdentifier)
        {
            return Data?.FirstOrDefault(x => x.MachineIdentifier == machineIdentifier);
        }

        public PlexServerSettingsModel GetPlexServerSettings(int plexServerId)
        {
            return Data?.FirstOrDefault(x => x.PlexServerId == plexServerId);
        }

        public override IServerSettings GetValues()
        {
            return new ServerSettings
            {
                Data = Data,
            };
        }

        public IObservable<PlexServerSettingsModel> ServerSettings(int plexServerId)
        {
            return _serverSettingsUpdated.AsObservable().Where(x => x.PlexServerId == plexServerId);
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

        public override Result SetFromJson(JsonElement settingsJsonElement)
        {
            var jsonSettings = GetJsonSettingsModule(settingsJsonElement);
            if (jsonSettings.IsFailed)
            {
                Reset();
                return jsonSettings;
            }

            if (!jsonSettings.Value.TryGetProperty(nameof(IServerSettings.Data), out JsonElement jsonValueElement))
            {
                Log.Error($"Failed to read property {nameof(IServerSettings.Data)} from {Name}");
                Reset();
            }

            var serverSettings = jsonValueElement.GetRawText();

            try
            {
                Data = JsonSerializer.Deserialize<List<PlexServerSettingsModel>>(serverSettings, DefaultJsonSerializerOptions.ConfigManagerOptions);
            }
            catch (Exception e)
            {
                Result.Fail(new ExceptionalError(e)).LogError();
                Log.Error($"Failed to read {Name}, will reset config now");
                Reset();
            }

            return Result.Ok();
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

        public Result Update(IServerSettingsModule sourceSettings)
        {
            Data = sourceSettings.Data;
            return Result.Ok();
        }

        public void EnsureAllServersHaveASettingsEntry()
        {
            Data = CreateServerSettingsFromDb();
            EmitModuleHasChanged(GetValues());
        }

        #endregion

        #region Private Methods

        private List<PlexServerSettingsModel> CreateServerSettingsFromDb()
        {
            var plexServers = _mediator.Send(new GetAllPlexServersQuery()).Result;
            if (plexServers.IsFailed)
            {
                plexServers.LogError();
                return new List<PlexServerSettingsModel>();
            }

            var newList = new List<PlexServerSettingsModel>();
            foreach (var plexServer in plexServers.Value)
            {
                var index = Data.FindIndex(x => x.MachineIdentifier == plexServer.MachineIdentifier);
                if (index == -1)
                {
                    newList.Add(new PlexServerSettingsModel
                    {
                        MachineIdentifier = plexServer.MachineIdentifier,
                        DownloadSpeedLimit = 0,
                        PlexServerId = plexServer.Id,
                    });
                }
            }

            return newList.Concat(Data).ToList();
        }

        #endregion
    }
}