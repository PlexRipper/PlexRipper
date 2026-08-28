namespace Reaparr.Application;

public record CreateRadarrIntegrationRequest
{
    public required string Name { get; init; }
    public required string Url { get; init; }
    public required string ApiKey { get; init; }
    public required string Category { get; init; }
    public string? DownloadPath { get; init; }
}

public class CreateRadarrIntegrationRequestValidator : Validator<CreateRadarrIntegrationRequest>
{
    public CreateRadarrIntegrationRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(RadarrIntegration.NameMaxLength);
        RuleFor(x => x.Url)
            .NotEmpty()
            .MaximumLength(RadarrIntegration.BaseUrlMaxLength)
            .Must(IsHttpUrl)
            .WithMessage("URL must be an absolute http/https URL.");
        RuleFor(x => x.ApiKey).NotEmpty().MaximumLength(RadarrIntegration.ApiKeyMaxLength);
        RuleFor(x => x.Category).NotEmpty().MaximumLength(RadarrIntegration.CategoryMaxLength);
        RuleFor(x => x.DownloadPath).MaximumLength(RadarrIntegration.DownloadPathMaxLength);
    }

    private static bool IsHttpUrl(string value) =>
        Uri.TryCreate(value.TrimEnd('/'), UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
        && string.IsNullOrEmpty(uri.Query)
        && string.IsNullOrEmpty(uri.Fragment);
}

public class CreateRadarrIntegrationEndpoint : Endpoint<CreateRadarrIntegrationRequest, ResultDTO<RadarrIntegrationDTO>>
{
    private readonly IReaparrDbContext _dbContext;

    public CreateRadarrIntegrationEndpoint(IReaparrDbContext dbContext) => _dbContext = dbContext;

    public override void Configure()
    {
        Post(ApiRoutes.IntegrationController + "/Radarr/Configure");
        Roles(DefaultUserAppCredentials.DefaultAdminRole);
        Description(x =>
            x.Produces(StatusCodes.Status201Created, typeof(ResultDTO<RadarrIntegrationDTO>))
                .Produces(StatusCodes.Status400BadRequest, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(CreateRadarrIntegrationRequest req, CancellationToken ct)
    {
        var name = req.Name.Trim();
        var url = req.Url.Trim().TrimEnd('/');
        var apiKey = req.ApiKey.Trim();
        var category = req.Category.Trim();
        var downloadPath = string.IsNullOrWhiteSpace(req.DownloadPath) ? null : req.DownloadPath.Trim();

        var hasConflict = await _dbContext.RadarrIntegrations.AnyAsync(
            x => x.Name == name || x.Category == category || x.BaseUrl == url,
            ct
        );
        if (hasConflict)
        {
            await Send.FluentResult(
                ResultExtensions.Create400BadRequestResult(
                    "A Radarr integration with the same name, URL, or category already exists."
                ),
                ct
            );
            return;
        }

        var integration = new RadarrIntegration
        {
            Id = Guid.NewGuid(),
            Name = name,
            BaseUrl = url,
            RadarrApiKey = apiKey,
            ReaparrApiKey = Guid.NewGuid().ToString("N"),
            Category = category,
            DownloadPath = downloadPath,
            ProvisioningState = IntegrationProvisioningState.Unconfigured,
        };
        _dbContext.RadarrIntegrations.Add(integration);
        await _dbContext.SaveChangesAsync(ct);

        var result = Result.Ok(integration).Add201CreatedRequestSuccess("Radarr integration created successfully.");
        await Send.FluentResult(result, model => model.ToDTO(), ct);
    }
}
