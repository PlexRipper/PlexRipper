﻿﻿using FluentResults;
using PlexRipper.Domain;

namespace Settings.Contracts;

public interface IConfigManager : ISetup
{
    /// <summary>
    /// Writes all settings values in the <see cref="IUserSettings"/> to the json settings file.
    /// </summary>
    /// <returns>Is successful.</returns>
    Result SaveConfig();

    Result ResetConfig();

    Result LoadConfig();

    bool ConfigFileExists();
    
    /// <summary>
    /// Gets the location of the data folder for the application.
    /// </summary>
    /// <returns>The path to the data folder.</returns>
    string GetDataFolderLocation();
}
