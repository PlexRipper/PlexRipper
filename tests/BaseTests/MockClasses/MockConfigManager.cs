using System;
using Environment;
using FluentResults;
using Logging;
using PlexRipper.Application;
using PlexRipper.Settings.Models;

namespace PlexRipper.BaseTests.MockClasses
{
    public class MockConfigManager : IConfigManager
    {
        private readonly IFileSystem _fileSystem;

        private readonly IPathProvider _pathProvider;

        private readonly IUserSettings _userSettings;

        public MockConfigManager(IFileSystem fileSystem, IPathProvider pathProvider, IUserSettings userSettings)
        {
            _fileSystem = fileSystem;
            _pathProvider = pathProvider;
            _userSettings = userSettings;
        }

        public Result Setup()
        {
            Log.Information("Setting up default user config settings in integration mode");
            _userSettings.UpdateSettings(new SettingsModel());
            return Result.Ok();
        }

        public Result SaveConfig()
        {
            throw new NotImplementedException();
        }

        public Result ResetConfig()
        {
            throw new NotImplementedException();
        }

        public Result LoadConfig()
        {
            throw new NotImplementedException();
        }

        public bool ConfigFileExists()
        {
            return false;
        }
    }
}