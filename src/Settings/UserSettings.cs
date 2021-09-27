using System;
using System.Text.Json;
using Environment;
using FluentResultExtensions.lib;
using FluentResults;
using Logging;
using PlexRipper.Application.Common;
using PlexRipper.Domain;
using PlexRipper.Settings.Models;

namespace PlexRipper.Settings
{
    /// <inheritdoc cref="IUserSettings"/>
    public class UserSettings : SettingsModel, IUserSettings
    {
        private readonly IPathSystem _pathSystem;

        private readonly IFileSystem _fileSystem;

        #region Fields

        private readonly JsonSerializerOptions _jsonSerializerSettings = new()
        {
            WriteIndented = true,
            IncludeFields = false,
            PropertyNameCaseInsensitive = true,
        };

        #endregion

        public UserSettings(IPathSystem pathSystem, IFileSystem fileSystem)
        {
            _pathSystem = pathSystem;
            _fileSystem = fileSystem;
        }

        #region Methods

        #region Public

        public Result Setup()
        {
            Log.Information("Setting up UserSettings");

            var fileExistResult = _fileSystem.FileExists(_pathSystem.ConfigFileLocation);
            if (fileExistResult.IsFailed)
            {
                return fileExistResult;
            }

            if (!fileExistResult.Value)
            {
                Log.Information($"{_pathSystem.ConfigFileName} doesn't exist, will create new one now in {_pathSystem.ConfigDirectory}");
                return Save();
            }

            return Load();
        }

        public Result Load()
        {
            Log.Information("Loading UserSettings now.");

            try
            {
                var readResult = _fileSystem.FileReadAllText(_pathSystem.ConfigFileLocation);
                if (readResult.IsFailed)
                {
                    return readResult;
                }

                var loadedSettings = JsonSerializer.Deserialize<dynamic>(readResult.Value, _jsonSerializerSettings);
                Result result = SetFromJsonObject(loadedSettings);
                if (result.IsFailed)
                {
                    Log.Warning("Certain properties were missing or had missing or invalid values. Will correct those and re-save now!");
                    result.LogWarning();
                    Save();
                }
            }
            catch (Exception e)
            {
                Reset();
                return Result.Fail(new ExceptionalError("Failed to load the UserSettings to json file. Resetting now!", e)).LogError();
            }

            return Result.Ok().WithSuccess("UserSettings were loaded successfully!").LogInformation();
        }

        public Result Reset()
        {
            Log.Information("Resetting UserSettings");

            return UpdateSettings(new SettingsModel());
        }

        /// <inheritdoc/>
        public Result Save()
        {
            Log.Information("Saving UserSettings now.");

            string jsonString = JsonSerializer.Serialize(GetJsonObject(), _jsonSerializerSettings);
            var writeResult = _fileSystem.FileWriteAllText(_pathSystem.ConfigFileLocation, jsonString);
            if (writeResult.IsFailed)
            {
                return writeResult.WithError("Failed to write save settings").LogError();
            }

            return Result.Ok().WithSuccess("UserSettings were saved successfully!").LogInformation();
        }

        /// <inheritdoc/>
        public Result UpdateSettings(ISettingsModel sourceSettings)
        {
            FirstTimeSetup = sourceSettings.FirstTimeSetup;
            ActiveAccountId = sourceSettings.ActiveAccountId;
            DownloadSegments = sourceSettings.DownloadSegments;

            Language = sourceSettings.Language;
            AskDownloadMovieConfirmation = sourceSettings.AskDownloadMovieConfirmation;
            AskDownloadTvShowConfirmation = sourceSettings.AskDownloadTvShowConfirmation;
            AskDownloadSeasonConfirmation = sourceSettings.AskDownloadSeasonConfirmation;
            AskDownloadEpisodeConfirmation = sourceSettings.AskDownloadEpisodeConfirmation;

            TvShowViewMode = sourceSettings.TvShowViewMode;
            MovieViewMode = sourceSettings.MovieViewMode;

            ShortDateFormat = sourceSettings.ShortDateFormat;
            LongDateFormat = sourceSettings.LongDateFormat;
            TimeFormat = sourceSettings.TimeFormat;
            TimeZone = sourceSettings.TimeZone;
            ShowRelativeDates = sourceSettings.ShowRelativeDates;

            return Save();
        }

        #endregion

        #endregion
    }
}