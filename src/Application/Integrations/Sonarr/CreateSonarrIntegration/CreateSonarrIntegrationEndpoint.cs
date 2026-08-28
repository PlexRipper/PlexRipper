namespace Reaparr.Application;

public record CreateSonarrIntegrationRequest
{
    public required string Name { get; init; }
    public required string Url { get; init; }
    public required string ApiKey { get; init; }
    public required string Category { get; init; }
    public string? DownloadPath { get; init; }
}

public class CreateSonarrIntegrationRequestValidator : Validator<CreateSonarrIntegrationRequest>
{
    public CreateSonarrIntegrationRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(SonarrIntegration.NameMaxLength);
        RuleFor(x => x.Url)
            .NotEmpty()
            .MaximumLength(SonarrIntegration.BaseUrlMaxLength)
            .Must(IsHttpUrl)
            .WithMessage("URL must be an absolute http/https URL.");
        RuleFor(x => x.ApiKey).NotEmpty().MaximumLength(SonarrIntegration.ApiKeyMaxLength);
        RuleFor(x => x.Category).NotEmpty().MaximumLength(SonarrIntegration.CategoryMaxLength);
        RuleFor(x => x.DownloadPath).MaximumLength(SonarrIntegration.DownloadPathMaxLength);
    }

    private static bool IsHttpUrl(string value) =>
        Uri.TryCreate(value.TrimEnd('/'), UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
        && string.IsNullOrEmpty(uri.Query)
        && string.IsNullOrEmpty(uri.Fragment);
}

public class CreateSonarrIntegrationEndpoint : Endpoint<CreateSonarrIntegrationRequest, ResultDTO<SonarrIntegrationDTO>>
{
    private readonly IReaparrDbContext _dbContext;

    public CreateSonarrIntegrationEndpoint(IReaparrDbContext dbContext) => _dbContext = dbContext;

    public override void Configure()
    {
        Post(ApiRoutes.IntegrationController + "/Sonarr/Configure");
        Roles(DefaultUserAppCredentials.DefaultAdminRole);
        Description(x =>
            x.Produces(StatusCodes.Status201Created, typeof(ResultDTO<SonarrIntegrationDTO>))
                .Produces(StatusCodes.Status400BadRequest, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(CreateSonarrIntegrationRequest req, CancellationToken ct)
    {
        var name = req.Name.Trim();
        var url = req.Url.Trim().TrimEnd('/');
        var apiKey = req.ApiKey.Trim();
        var category = req.Category.Trim();
        var downloadPath = string.IsNullOrWhiteSpace(req.DownloadPath) ? null : req.DownloadPath.Trim();

        var hasConflict = await _dbContext.SonarrIntegrations.AnyAsync(
            x => x.Name == name || x.Category == category || x.BaseUrl == url,
            ct
        );
        if (hasConflict)
        {
            await Send.FluentResult(
                ResultExtensions.Create400BadRequestResult(
                    "A Sonarr integration with the same name, URL, or category already exists."
                ),
                ct
            );
            return;
        }

        var integration = new SonarrIntegration
        {
            Id = Guid.NewGuid(),
            Name = name,
            BaseUrl = url,
            SonarrApiKey = apiKey,
            ReaparrApiKey = Guid.NewGuid().ToString("N"),
            Category = category,
            DownloadPath = downloadPath,
            ProvisioningState = IntegrationProvisioningState.Unconfigured,
        };
        _dbContext.SonarrIntegrations.Add(integration);
        await _dbContext.SaveChangesAsync(ct);

        var result = Result.Ok(integration).Add201CreatedRequestSuccess("Sonarr integration created successfully.");
        await Send.FluentResult(result, model => model.ToDTO(), ct);
    }
}
