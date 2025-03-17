using Logging.Interface;

namespace Settings.Contracts;

public class TorznabSettingsModule : BaseSettingsModule<ITorznabSettings, TorznabSettingsDTO>
{
    public TorznabSettingsModule(
        IUserSettings userSettings,
        ILog log,
        IConfigManager configManager) : base(userSettings, log, configManager)
    {
    }

    private TorznabSettingsDTO CreateDefaultDTO()
    {
        return new TorznabSettingsDTO
        {
            Enabled = false,
            ApiKey = Guid.NewGuid().ToString("N").Substring(0, 16), // Generate a random API key
            RequireApiKey = true,
            MaxResults = 100,
            DownloadDirectory = string.Empty,
            AutoStart = true
        };
    }

    public override TorznabSettingsDTO GetDTO()
    {
        var dto = UserSettings.TorznabSettings;
        return dto ?? CreateDefaultDTO();
    }

    /// <summary>
    /// Validates if the API key is valid
    /// </summary>
    /// <param name="apiKey">The API key to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    public bool IsValidApiKey(string? apiKey)
    {
        // If API key is not required, always return true
        if (!GetDTO().RequireApiKey)
        {
            return true;
        }

        // If API key is required but none provided, return false
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return false;
        }

        // Compare the provided API key with the stored one
        return apiKey == GetDTO().ApiKey;
    }

    /// <summary>
    /// Get the download directory path for Torznab downloads
    /// </summary>
    /// <returns>The download directory path</returns>
    public string GetDownloadDirectory()
    {
        var directory = GetDTO().DownloadDirectory;
        if (string.IsNullOrWhiteSpace(directory))
        {
            // Fallback to default download directory
            return Path.Combine(ConfigManager.GetDataFolderLocation(), "TorznabDownloads");
        }
        return directory;
    }
    
    /// <summary>
    /// Checks if the Torznab API integration is enabled
    /// </summary>
    /// <returns>True if enabled, false otherwise</returns>
    public bool IsEnabled()
    {
        return GetDTO().Enabled;
    }
    
    /// <summary>
    /// Enable or disable the Torznab API integration
    /// </summary>
    /// <param name="enabled">True to enable, false to disable</param>
    /// <returns>Result of the operation</returns>
    public async Task<Result> SetEnabled(bool enabled)
    {
        var dto = GetDTO();
        dto.Enabled = enabled;
        return await UserSettings.Update(dto);
    }
    
    /// <summary>
    /// Set a new API key for the Torznab API integration
    /// </summary>
    /// <param name="apiKey">The new API key</param>
    /// <returns>Result of the operation</returns>
    public async Task<Result> SetApiKey(string apiKey)
    {
        var dto = GetDTO();
        dto.ApiKey = apiKey;
        return await UserSettings.Update(dto);
    }
    
    /// <summary>
    /// Set whether the API key is required for Torznab API requests
    /// </summary>
    /// <param name="required">True if required, false otherwise</param>
    /// <returns>Result of the operation</returns>
    public async Task<Result> SetRequireApiKey(bool required)
    {
        var dto = GetDTO();
        dto.RequireApiKey = required;
        return await UserSettings.Update(dto);
    }
    
    /// <summary>
    /// Set the maximum number of results to return for Torznab API requests
    /// </summary>
    /// <param name="maxResults">The maximum number of results</param>
    /// <returns>Result of the operation</returns>
    public async Task<Result> SetMaxResults(int maxResults)
    {
        var dto = GetDTO();
        dto.MaxResults = Math.Max(1, Math.Min(1000, maxResults)); // Clamp between 1 and 1000
        return await UserSettings.Update(dto);
    }
    
    /// <summary>
    /// Set the download directory for Torznab downloads
    /// </summary>
    /// <param name="directory">The download directory path</param>
    /// <returns>Result of the operation</returns>
    public async Task<Result> SetDownloadDirectory(string directory)
    {
        var dto = GetDTO();
        dto.DownloadDirectory = directory;
        return await UserSettings.Update(dto);
    }
    
    /// <summary>
    /// Set whether downloads should be automatically started
    /// </summary>
    /// <param name="autoStart">True to auto-start downloads, false otherwise</param>
    /// <returns>Result of the operation</returns>
    public async Task<Result> SetAutoStart(bool autoStart)
    {
        var dto = GetDTO();
        dto.AutoStart = autoStart;
        return await UserSettings.Update(dto);
    }
}
