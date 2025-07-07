using Application.Contracts;

namespace PlexRipper.Application;

public class GenerateTorznabApiKeyEndpoint : BaseEndpointWithoutRequest<GenerateTorznabApiKeyResponse>
{
    private readonly ITorznabAuthenticationService _torznabAuth;

    public GenerateTorznabApiKeyEndpoint(ITorznabAuthenticationService torznabAuth)
    {
        _torznabAuth = torznabAuth;
    }

    public override void Configure()
    {
        Post("/api/settings/torznab/generate-api-key");
        Roles("Admin", "User");
        Summary(s =>
        {
            s.Summary = "Generate new Torznab API key";
            s.Description = "Generate a new API key for Torznab authentication";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var newApiKey = _torznabAuth.GenerateNewApiKey();
        
        var response = new GenerateTorznabApiKeyResponse
        {
            ApiKey = newApiKey
        };
        
        await SendOkAsync(response, ct);
    }
}

public class GenerateTorznabApiKeyResponse
{
    public string ApiKey { get; set; } = "";
}