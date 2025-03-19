using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Environment;
using FluentResults;
using Logging.Interface;

namespace Settings.Contracts;

public record TorznabSettingsModule : BaseSettingsModule<TorznabSettingsModule>, ITorznabSettings
{
    private bool _enabled = false;
    private string _apiKey = string.Empty;
    private bool _requireApiKey = true;
    private int _maxResults = 100;
    private string _downloadDirectory = string.Empty;
    private bool _autoStart = true;
    private List<int> _includedServerIds = new();
    private bool _searchAllServers = true;

    // Default constructor needed for serialization
    public TorznabSettingsModule()
    {
        _apiKey = Guid.NewGuid().ToString("N").Substring(0, 16); // Generate a random API key
    }

    public static TorznabSettingsModule Create() =>
        new()
        {
            Enabled = false,
            ApiKey = Guid.NewGuid().ToString("N").Substring(0, 16),
            RequireApiKey = true,
            MaxResults = 100,
            DownloadDirectory = string.Empty,
            AutoStart = true,
            IncludedServerIds = new List<int>(),
            SearchAllServers = true,
        };

    // Property implementations from ITorznabSettings
    public bool Enabled 
    { 
        get => _enabled; 
        set => SetProperty(ref _enabled, value); 
    }
    
    public string ApiKey 
    { 
        get => _apiKey; 
        set => SetProperty(ref _apiKey, value); 
    }
    
    public bool RequireApiKey 
    { 
        get => _requireApiKey; 
        set => SetProperty(ref _requireApiKey, value); 
    }
    
    public int MaxResults 
    { 
        get => _maxResults; 
        set => SetProperty(ref _maxResults, Math.Max(1, Math.Min(1000, value))); 
    }
    
    public string DownloadDirectory 
    { 
        get => _downloadDirectory; 
        set => SetProperty(ref _downloadDirectory, value); 
    }
    
    public bool AutoStart 
    { 
        get => _autoStart; 
        set => SetProperty(ref _autoStart, value); 
    }
    
    public List<int> IncludedServerIds 
    { 
        get => _includedServerIds; 
        set => SetProperty(ref _includedServerIds, value); 
    }
    
    public bool SearchAllServers 
    { 
        get => _searchAllServers; 
        set => SetProperty(ref _searchAllServers, value); 
    }

    private readonly IPathProvider? _pathProvider;
    
    // Constructor with optional IPathProvider dependency
    public TorznabSettingsModule(IPathProvider? pathProvider = null)
    {
        _pathProvider = pathProvider;
        _apiKey = Guid.NewGuid().ToString("N").Substring(0, 16); // Generate a random API key
    }

    /// <summary>
    /// Validates if the API key is valid
    /// </summary>
    /// <param name="apiKey">The API key to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    public bool IsValidApiKey(string? apiKey)
    {
        // If API key is not required, always return true
        if (!RequireApiKey)
        {
            return true;
        }

        // If API key is required but none provided, return false
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return false;
        }

        // Compare the provided API key with the stored one
        return apiKey == ApiKey;
    }

    /// <summary>
    /// Get the download directory path for Torznab downloads
    /// </summary>
    /// <returns>The download directory path</returns>
    public string GetEffectiveDownloadDirectory()
    {
        if (string.IsNullOrWhiteSpace(DownloadDirectory) && _pathProvider != null)
        {
            // Fallback to default download directory
            return Path.Combine(_pathProvider.ConfigDirectory, "TorznabDownloads");
        }
        return DownloadDirectory;
    }
    
    /// <summary>
    /// Add a server ID to the list of included servers
    /// </summary>
    /// <param name="serverId">Server ID to add</param>
    public void AddIncludedServerId(int serverId)
    {
        if (!IncludedServerIds.Contains(serverId))
        {
            var newList = new List<int>(IncludedServerIds) { serverId };
            IncludedServerIds = newList;
        }
    }
    
    /// <summary>
    /// Remove a server ID from the list of included servers
    /// </summary>
    /// <param name="serverId">Server ID to remove</param>
    public void RemoveIncludedServerId(int serverId)
    {
        if (IncludedServerIds.Contains(serverId))
        {
            var newList = new List<int>(IncludedServerIds);
            newList.Remove(serverId);
            IncludedServerIds = newList;
        }
    }
    
    /// <summary>
    /// Check if a server ID is included in searches
    /// </summary>
    /// <param name="serverId">Server ID to check</param>
    /// <returns>True if the server is included in searches</returns>
    public bool IsServerIncluded(int serverId)
    {
        return SearchAllServers || IncludedServerIds.Contains(serverId);
    }
}
