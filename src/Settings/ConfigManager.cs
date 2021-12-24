using System;
using System.Text.Json;
using Environment;
using FluentResults;
using Logging;
using PlexRipper.Application;

namespace PlexRipper.Settings
{
    public class ConfigManager : IConfigManager
    {
        #region Fields

        private readonly IFileSystem _fileSystem;

        private readonly IPathProvider _pathProvider;

        private readonly IUserSettings _userSettings;

        private readonly JsonSerializerOptions _jsonSerializerSettings = new()
        {
            WriteIndented = true,
            IncludeFields = false,
            PropertyNameCaseInsensitive = true,
        };

        #endregion

        #region Constructor

        public ConfigManager(IFileSystem fileSystem, IPathProvider pathProvider, IUserSettings userSettings)
        {
            _fileSystem = fileSystem;
            _pathProvider = pathProvider;
            _userSettings = userSettings;
        }

        #endregion

        #region Public Methods

        public Result Setup()
        {
            _userSettings.SettingsUpdated.Subscribe(_ => SaveConfig());

            Log.Information($"Checking if {_pathProvider.ConfigFileName} exists");

            if (!ConfigFileExists())
            {
                Log.Information($"{_pathProvider.ConfigFileName} doesn't exist, will create new one now in {_pathProvider.ConfigDirectory}");
                return CreateConfigFile();
            }

            var loadResult = LoadConfig();
            return loadResult.IsFailed ? loadResult : Result.Ok();
        }

        public Result LoadConfig()
        {
            try
            {
                Log.Information("Loading user config settings now.");

                var readResult = _fileSystem.FileReadAllText(_pathProvider.ConfigFileLocation);
                if (readResult.IsFailed)
                {
                    return readResult;
                }

                var loadedSettings = JsonSerializer.Deserialize<dynamic>(readResult.Value, _jsonSerializerSettings);
                var setFromJsonResult = _userSettings.SetFromJsonObject(loadedSettings);
                if (setFromJsonResult.IsFailed)
                {
                    Log.Warning("Certain properties were missing or had missing or invalid values. Will correct those and re-save now!");
                    setFromJsonResult.LogWarning();
                    SaveConfig();
                }

                return Result.Ok().WithSuccess("UserSettings were loaded successfully!").LogInformation();
            }
            catch (Exception e)
            {
                return Result.Fail(new ExceptionalError("Failed to load the UserSettings to json file", e)).LogError();
            }
        }

        public Result ResetConfig()
        {
            _userSettings.Reset();
            var saveResult = SaveConfig();
            if (saveResult.IsFailed)
            {
                saveResult.WithError(new Error("Failed to save a new config after resetting")).LogError();
            }

            return Result.Ok();
        }

        public Result SaveConfig()
        {
            Log.Information("Saving user config settings now.");

            var jsonSettings = JsonSerializer.Serialize(_userSettings.GetJsonSettingsObject(), _jsonSerializerSettings);

            var writeResult = WriteToConfigFile(jsonSettings);
            if (writeResult.IsFailed)
            {
                return writeResult;
            }

            return Result.Ok().WithSuccess("UserSettings were saved successfully!").LogInformation();
        }

        private Result WriteToConfigFile(string jsonSettingsString)
        {
            var writeResult = _fileSystem.FileWriteAllText(_pathProvider.ConfigFileLocation, jsonSettingsString);
            return writeResult.IsFailed ? writeResult.WithError("Failed to write save settings").LogError() : Result.Ok();
        }

        #endregion

        #region Private Methods

        private bool ConfigFileExists()
        {
            return _fileSystem.FileExists(_pathProvider.ConfigFileLocation);
        }

        private Result CreateConfigFile()
        {
            var settingsObject = _userSettings.GetJsonSettingsObject();
            if (settingsObject.IsFailed)
            {
                return settingsObject.LogError();
            }

            var writeResult = WriteToConfigFile(settingsObject.Value);
            return writeResult.IsFailed ? writeResult.LogError() : Result.Ok();
        }

        #endregion
    }
}