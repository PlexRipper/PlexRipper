using Application.Contracts;
using Settings.Contracts;

namespace PlexRipper.Application;

public class UpdateTorznabSettingsEndpoint : BaseEndpoint<TorznabSettingsDTO, TorznabSettingsDTO>
{
    private readonly IUserSettings _userSettings;
    private readonly ITorznabAuthenticationService _torznabAuth;

    public UpdateTorznabSettingsEndpoint(IUserSettings userSettings, ITorznabAuthenticationService torznabAuth)
    {
        _userSettings = userSettings;
        _torznabAuth = torznabAuth;
    }

    public override void Configure()
    {
        Put("/api/settings/torznab");
        Roles("Admin");
        Summary(s =>
        {
            s.Summary = "Update Torznab settings";
            s.Description = "Update Torznab configuration settings";
        });
    }

    public override async Task<TorznabSettingsDTO> ExecuteAsync(TorznabSettingsDTO req, CancellationToken ct)
    {
        // Generate new API key if requested or if empty
        if (string.IsNullOrEmpty(req.ApiKey))
        {
            req.ApiKey = _torznabAuth.GenerateNewApiKey();
        }

        // Validate settings
        if (req.MaxResultsPerRequest <= 0 || req.MaxResultsPerRequest > 1000)
        {
            req.MaxResultsPerRequest = 100;
        }

        if (req.SearchTimeoutSeconds <= 0 || req.SearchTimeoutSeconds > 300)
        {
            req.SearchTimeoutSeconds = 30;
        }

        // Update the settings
        var updatedModule = req.ToModel();
        _userSettings.TorznabSettings.Update(updatedModule);

        return _userSettings.TorznabSettings.ToDTO();
    }
}