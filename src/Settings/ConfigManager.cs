﻿using System.IO.Abstractions;
using Environment;
using Logging.Interface;
using Settings.Contracts;

namespace PlexRipper.Settings;

public class ConfigManager : IConfigManager
{
    #region Fields

    private readonly ILog _log;
    private readonly IPathProvider _pathProvider;

    private readonly IUserSettings _userSettings;
    private readonly IFile _file;
    private readonly IDirectory _directory;

    #endregion

    #region Constructor

    public ConfigManager(
        ILog log,
        IPathProvider pathProvider,
        IUserSettings userSettings,
        IFile file,
        IDirectory directory
    )
    {
        _log = log;
        _pathProvider = pathProvider;
        _userSettings = userSettings;
        _file = file;
        _directory = directory;
    }

    #endregion

    #region Public Methods

    public Result Setup()
    {
        _userSettings.SettingsUpdated.Subscribe(_ => SaveConfig());

        _log.Here()
            .Information(
                "Checking if {ConfigFileName} exists at {ConfigDirectory}",
                _pathProvider.ConfigFileName,
                _pathProvider.ConfigDirectory
            );

        var configDirectoryExistsResult = Result.Try(() => _directory.Exists(_pathProvider.ConfigDirectory));
        if (configDirectoryExistsResult.IsFailed)
            return configDirectoryExistsResult.LogFatal();

        if (configDirectoryExistsResult.Value)
            _log.Information("Config directory exists, will use {ConfigDirectory}", _pathProvider.ConfigDirectory);
        else
        {
            _log.Information(
                "Config directory does not exist, will create now at {ConfigDirectory}",
                _pathProvider.ConfigDirectory
            );
            var createResult = Result.Try(() => _directory.CreateDirectory(_pathProvider.ConfigDirectory));
            if (createResult.IsFailed)
            {
                _log.Fatal("Failed to create config directory at {ConfigDirectory}", _pathProvider.ConfigDirectory);
                return createResult.LogFatal();
            }

            _log.Debug("Directory: {ConfigDirectory} created!", _pathProvider.ConfigDirectory);
        }

        if (!ConfigFileExists())
        {
            _log.Here()
                .Information(
                    "{ConfigFileName} doesn't exist, will create new one now in {ConfigDirectory}",
                    _pathProvider.ConfigFileName,
                    _pathProvider.ConfigDirectory
                );
            return SaveConfig();
        }

        var loadResult = LoadConfig();
        return loadResult.IsFailed ? loadResult : Result.Ok();
    }

    public virtual Result LoadConfig()
    {
        _log.DebugLine("Loading user config settings now");
        var readResult = ReadFromConfigFile();
        if (readResult.IsFailed)
        {
            _log.Information(
                "Resetting {ConfigFileName} because it could not be loaded correctly",
                _pathProvider.ConfigFileName
            );
            return ResetConfig();
        }

        try
        {
            var cleanedJson = readResult.Value.Replace("\r\n", "");
            var loadedSettings = UserSettingsSerializer.Deserialize(cleanedJson);

            _userSettings.UpdateSettings(loadedSettings);

            return Result.Ok().WithSuccess("UserSettings were loaded successfully!").LogInformation();
        }
        catch (Exception e)
        {
            Result.Fail(new ExceptionalError(e)).LogError();
            _log.Error("Failed to JSON parse the contents from {ConfigFileName}", _pathProvider.ConfigFileName);
            _log.Error("Contents: {Contents}", readResult.Value);
            return ResetConfig();
        }
    }

    public virtual Result ResetConfig()
    {
        _userSettings.Reset();
        var saveResult = SaveConfig();
        if (saveResult.IsFailed)
            saveResult.WithError(new Error("Failed to save a new config after resetting")).LogError();

        return Result.Ok();
    }

    public virtual Result SaveConfig()
    {
        _log.DebugLine("Saving user config settings now");

        var jsonSettings = UserSettingsSerializer.Serialize(_userSettings);

        var writeResult = WriteToConfigFile(jsonSettings);

        if (writeResult.IsFailed)
            return writeResult;

        _log.DebugLine("UserSettings were saved successfully!");

        return Result.Ok().WithSuccess("UserSettings were saved successfully!").LogInformation();
    }

    public virtual bool ConfigFileExists() => _file.Exists(_pathProvider.ConfigFileLocation);
    
    public virtual string GetDataFolderLocation()
    {
        // Use the ConfigDirectory as the data folder location
        // This is a reasonable assumption since configuration data is typically stored here
        return _pathProvider.ConfigDirectory;
    }

    #endregion

    #region Private Methods

    private Result WriteToConfigFile(string jsonSettingsString)
    {
        var writeResult = Result.Try(() => _file.WriteAllText(_pathProvider.ConfigFileLocation, jsonSettingsString));
        return writeResult.IsFailed ? writeResult.WithError("Failed to write config settings").LogError() : Result.Ok();
    }

    private Result<string> ReadFromConfigFile()
    {
        var readResult = Result.Try(() => _file.ReadAllText(_pathProvider.ConfigFileLocation));
        if (readResult.IsFailed)
        {
            _log.Here()
                .Error(
                    "Failed to read {ConfigFileName} from {ConfigDirectory}",
                    _pathProvider.ConfigFileName,
                    _pathProvider.ConfigDirectory
                );
            readResult.LogError();
        }

        return readResult;
    }

    #endregion
}
