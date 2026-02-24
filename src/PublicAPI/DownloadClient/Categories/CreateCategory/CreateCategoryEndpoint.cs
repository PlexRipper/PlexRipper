using FastEndpoints;

namespace Reaparr.PublicAPI.CreateCategory;

public record CreateCategoryRequest
{
    [BindFrom("category")]
    public required string Category { get; init; }

    [BindFrom("savePath")]
    public string? SavePath { get; init; }
}

public class CreateCategoryEndpoint : Endpoint<CreateCategoryRequest>
{
    private readonly ILogger _log;

    public CreateCategoryEndpoint(ILogger logger)
    {
        _log = logger.ForContext<CreateCategoryEndpoint>();
    }

    public override void Configure()
    {
        Post(PublicApiRoutes.DownloadClient + "/torrents/createCategory");
        Description(x => x.IsDownloadClient());
        AllowFormData(urlEncoded: true);
        AllowAnonymous();
        PreProcessor<DownloadClientAuthenticationPreProcessor<CreateCategoryRequest>>();
    }

    public override async Task HandleAsync(CreateCategoryRequest req, CancellationToken ct)
    {
        _log.Here().DebugApiCall(HttpContext, req);
        await Send.OkAsync(cancellation: ct);
    }
}
