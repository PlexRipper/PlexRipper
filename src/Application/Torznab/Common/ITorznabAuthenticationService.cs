using Settings.Contracts;

namespace PlexRipper.Application;

public interface ITorznabAuthenticationService
{
    bool IsEnabled { get; }
    bool ValidateApiKey(string? apiKey);
    string GenerateNewApiKey();
}

public class TorznabAuthenticationService : ITorznabAuthenticationService
{
    private readonly IUserSettings _userSettings;

    public TorznabAuthenticationService(IUserSettings userSettings)
    {
        _userSettings = userSettings;
    }

    public bool IsEnabled => _userSettings.TorznabSettings.IsEnabled;

    public bool ValidateApiKey(string? apiKey)
    {
        if (!IsEnabled) return false;
        if (string.IsNullOrEmpty(apiKey)) return false;
        
        return apiKey == _userSettings.TorznabSettings.ApiKey;
    }

    public string GenerateNewApiKey()
    {
        return Guid.NewGuid().ToString("N");
    }
}