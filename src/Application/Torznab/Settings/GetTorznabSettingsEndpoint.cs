using Application.Contracts;
using Settings.Contracts;

namespace PlexRipper.Application;

public class GetTorznabSettingsEndpoint : BaseEndpoint<EmptyRequest, TorznabSettingsDTO>
{
    private readonly IUserSettings _userSettings;

    public GetTorznabSettingsEndpoint(IUserSettings userSettings)
    {
        _userSettings = userSettings;
    }

    public override void Configure()
    {
        Get("/api/settings/torznab");
        Roles("Admin", "User");
        Summary(s =>
        {
            s.Summary = "Get Torznab settings";
            s.Description = "Retrieve current Torznab configuration settings";
        });
    }

    public override async Task<TorznabSettingsDTO> ExecuteAsync(EmptyRequest req, CancellationToken ct)
    {
        return _userSettings.TorznabSettings.ToDTO();
    }
}