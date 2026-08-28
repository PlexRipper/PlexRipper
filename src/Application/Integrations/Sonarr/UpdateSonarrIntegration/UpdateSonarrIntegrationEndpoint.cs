namespace Reaparr.Application;

public record UpdateSonarrIntegrationRequest
{
    [RouteParam]
    public Guid IntegrationId { get; init; }
    public required string Name { get; init; }
    public required string Url { get; init; }
    public required string ApiKey { get; init; }
    public required string Category { get; init; }
    public string? DownloadPath { get; init; }
}

public class UpdateSonarrIntegrationRequestValidator : Validator<UpdateSonarrIntegrationRequest>
{
    public UpdateSonarrIntegrationRequestValidator()
    {
        RuleFor(x => x.IntegrationId).NotEmpty();
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

public class UpdateSonarrIntegrationEndpoint : Endpoint<UpdateSonarrIntegrationRequest, ResultDTO<SonarrIntegrationDTO>>
{
    private readonly IReaparrDbContext _dbContext;

    public UpdateSonarrIntegrationEndpoint(IReaparrDbContext dbContext) => _dbContext = dbContext;

    public override void Configure()
    {
        Put(ApiRoutes.IntegrationController + "/Sonarr/{integrationId:guid}/Configure");
        Roles(DefaultUserAppCredentials.DefaultAdminRole);
        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<SonarrIntegrationDTO>))
                .Produces(StatusCodes.Status400BadRequest, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status404NotFound, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(UpdateSonarrIntegrationRequest req, CancellationToken ct)
    {
        var integration = await _dbContext
            .SonarrIntegrations.AsTracking()
            .SingleOrDefaultAsync(x => x.Id == req.IntegrationId, ct);
        if (integration is null)
        {
            await Send.FluentResult(ResultExtensions.EntityNotFound(nameof(SonarrIntegration), req.IntegrationId), ct);
            return;
        }

        var name = req.Name.Trim();
        var url = req.Url.Trim().TrimEnd('/');
        var apiKey = req.ApiKey.Trim();
        var category = req.Category.Trim();
        var downloadPath = string.IsNullOrWhiteSpace(req.DownloadPath) ? null : req.DownloadPath.Trim();

        var hasConflict = await _dbContext.SonarrIntegrations.AnyAsync(
            x => x.Id != integration.Id && (x.Name == name || x.Category == category || x.BaseUrl == url),
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

        if (
            integration.Category != category
            && integration.ProvisioningState == IntegrationProvisioningState.Configured
        )
            integration.ProvisioningState = IntegrationProvisioningState.ChangesPending;

        integration.Name = name;
        integration.BaseUrl = url;
        integration.SonarrApiKey = apiKey;
        integration.Category = category;
        integration.DownloadPath = downloadPath;
        await _dbContext.SaveChangesAsync(ct);

        await Send.FluentResult(Result.Ok(integration), model => model.ToDTO(), ct);
    }
}
