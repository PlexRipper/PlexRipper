using System;
using System.IO;
using System.Text.Json;
using Environment;
using FluentResults;
using Logging;
using PlexRipper.Application;
using PlexRipper.Domain.Config;

namespace PlexRipper.Settings
{
    public class ConfigManager : IConfigManager
    {
        #region Fields

        private readonly IFileSystem _fileSystem;

        private readonly IDirectorySystem _directorySystem;

        private readonly IPathProvider _pathProvider;

        private readonly IUserSettings _userSettings;

        #endregion

        #region Constructor

        public ConfigManager(IFileSystem fileSystem, IDirectorySystem directorySystem, IPathProvider pathProvider, IUserSettings userSettings)
        {
            _fileSystem = fileSystem;
            _directorySystem = directorySystem;
            _pathProvider = pathProvider;
            _userSettings = userSettings;
        }

        #endregion

        #region Public Methods

        public Result Setup()
        {
            _userSettings.SettingsUpdated.Subscribe(_ => SaveConfig());

            Log.Information($"Checking if \"{_pathProvider.ConfigFileName}\" exists at \"{_pathProvider.ConfigDirectory}\"");

            var configDirectoryExistsResult = _directorySystem.Exists(_pathProvider.ConfigDirectory);
            if (configDirectoryExistsResult.IsFailed)
            {
                return configDirectoryExistsResult.LogFatal();
            }

            if (configDirectoryExistsResult.Value)
            {
                Log.Information($"Config directory exists, will use \"{_pathProvider.ConfigDirectory}\"");
            }
            else
            {
                Log.Information($"Config directory does not exist, will create now at \"{_pathProvider.ConfigDirectory}\".");
                var createResult = _directorySystem.CreateDirectory(_pathProvider.ConfigDirectory);
                if (createResult.IsFailed)
                {
                    Log.Fatal($"Failed to create config directory at \"{_pathProvider.ConfigDirectory}\"");
                    return createResult.LogFatal();
                }

                Log.Debug($"Directory: \"{_pathProvider.ConfigDirectory}\" created!");
            }

            if (!ConfigFileExists())
            {
                Log.Information($"\"{_pathProvider.ConfigFileName}\" doesn't exist, will create new one now in \"{_pathProvider.ConfigDirectory}\"");
                return SaveConfig();
            }

            Result loadResult = LoadConfig();
            return loadResult.IsFailed ? loadResult : Result.Ok();
        }

        public virtual Result LoadConfig()
        {
            Log.Information("Loading user config settings now.");
            var readResult = ReadFromConfigFile();
            if (readResult.IsFailed)
            {
                Log.Information($"Resetting {_pathProvider.ConfigFileName} because it could not be loaded correctly");
                return ResetConfig();
            }

            try
            {
                var cleanedJson = readResult.Value.Replace("\r\n", "");
                var loadedSettings = JsonSerializer.Deserialize<JsonElement>(cleanedJson, DefaultJsonSerializerOptions.ConfigManagerOptions);
                Result setFromJsonResult = _userSettings.SetFromJsonObject(loadedSettings);
                if (setFromJsonResult.IsFailed)
                {
                    Log.Warning("Certain properties were missing or had missing or invalid values. Will correct those and re-save now!");
                    setFromJsonResult.LogWarning();
                    return ResetConfig();
                }

                return Result.Ok().WithSuccess("UserSettings were loaded successfully!").LogInformation();
            }
            catch (Exception e)
            {
                Result.Fail(new ExceptionalError(e)).LogError();
                Log.Error($"Failed to JSON parse the contents from {_pathProvider.ConfigFileName}");
                Log.Error($"Contents: {readResult.Value}");
                return ResetConfig();
            }
        }

        public virtual Result ResetConfig()
        {
            _userSettings.Reset();
            Result saveResult = SaveConfig();
            if (saveResult.IsFailed)
            {
                saveResult.WithError(new Error("Failed to save a new config after resetting")).LogError();
            }

            return Result.Ok();
        }

        public virtual Result SaveConfig()
        {
            Log.Information("Saving user config settings now.");

            var jsonSettings = GetJsonSettingsObject();
            if (jsonSettings.IsFailed)
            {
                return jsonSettings.ToResult();
            }

            Result writeResult = WriteToConfigFile(jsonSettings.Value);
            if (writeResult.IsFailed)
            {
                return writeResult;
            }

            return Result.Ok().WithSuccess("UserSettings were saved successfully!").LogInformation();
        }

        public virtual bool ConfigFileExists()
        {
            return _fileSystem.FileExists(_pathProvider.ConfigFileLocation);
        }

        #endregion

        #region Private Methods

        private Result WriteToConfigFile(string jsonSettingsString)
        {
            Result writeResult = _fileSystem.FileWriteAllText(_pathProvider.ConfigFileLocation, jsonSettingsString);
            return writeResult.IsFailed ? writeResult.WithError("Failed to write config settings").LogError() : Result.Ok();
        }

        private Result<string> ReadFromConfigFile()
        {
            var readResult = _fileSystem.FileReadAllText(_pathProvider.ConfigFileLocation);
            if (readResult.IsFailed)
            {
                Log.Error($"Failed to read {_pathProvider.ConfigFileName} from {_pathProvider.ConfigDirectory}");
                readResult.LogError();
            }

            return readResult;
        }

        private Result<string> GetJsonSettingsObject()
        {
            try
            {
                return Result.Ok(JsonSerializer.Serialize(_userSettings.GetSettingsModel(), DefaultJsonSerializerOptions.ConfigManagerOptions));
            }
            catch (Exception e)
            {
                return Result.Fail(new ExceptionalError(e)).LogError();
            }
        }

        #endregion
    }
}