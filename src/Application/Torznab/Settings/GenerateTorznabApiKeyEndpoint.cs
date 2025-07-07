using Application.Contracts;

namespace PlexRipper.Application;

public class GenerateTorznabApiKeyEndpoint : BaseEndpoint<EmptyRequest, GenerateTorznabApiKeyResponse>
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

    public override async Task<GenerateTorznabApiKeyResponse> ExecuteAsync(EmptyRequest req, CancellationToken ct)
    {
        var newApiKey = _torznabAuth.GenerateNewApiKey();
        
        return new GenerateTorznabApiKeyResponse
        {
            ApiKey = newApiKey
        };
    }
}

public class GenerateTorznabApiKeyResponse
{
    public string ApiKey { get; set; } = "";
}