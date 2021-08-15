using System;
using System.Text.Json;
using FluentResults;
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
                SetFromJsonObject(loadedSettings);
            }
            catch (Exception e)
            {
                return Result.Fail(new ExceptionalError("Failed to load the UserSettings to json file.", e)).LogError();
            }

            return Result.Ok().WithSuccess("UserSettings were loaded successfully!").LogInformation();
        }

        public Result Reset()
        {
            Log.Information("Resetting UserSettings");

            SetDefaultValues();

            var saveResult = Save();
            return saveResult.IsFailed ? saveResult : Result.Ok();
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