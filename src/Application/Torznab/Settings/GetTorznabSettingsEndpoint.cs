using Application.Contracts;
using Settings.Contracts;

namespace PlexRipper.Application;

public class GetTorznabSettingsEndpoint : BaseEndpointWithoutRequest<TorznabSettingsDTO>
{
    private readonly IUserSettings _userSettings;

    public GetTorznabSettingsEndpoint(IUserSettings userSettings)
    {
        _userSettings = userSettings;
    }

    public override void Configure()
    {
        Get("/api/settings/torznab");
        Roles("Admin");
        Summary(s =>
        {
            s.Summary = "Get Torznab settings";
            s.Description = "Retrieve current Torznab configuration settings";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await SendOkAsync(_userSettings.TorznabSettings.ToDTO(), ct);
    }
}