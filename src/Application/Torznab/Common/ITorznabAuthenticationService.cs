using Settings.Contracts;
using System.Security.Cryptography;
using System.Text;

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
        
        var storedKey = _userSettings.TorznabSettings.ApiKey;
        if (string.IsNullOrEmpty(storedKey)) return false;
        
        // Use constant-time comparison to prevent timing attacks
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(apiKey), 
            Encoding.UTF8.GetBytes(storedKey));
    }

    public string GenerateNewApiKey()
    {
        return Guid.NewGuid().ToString("N");
    }
}