using System.Net.Mime;
using FastEndpoints;
using FluentValidation;
using Reaparr.Application.Contracts;
using Reaparr.PlexApi.Contracts;

namespace Reaparr.Application;

public record GetPlexMediaThumbnailImageEndpointRequest
{
    [QueryParam, BindFrom("plexServerId")]
    public required int PlexServerId { get; init; }

    [QueryParam, BindFrom("plexKey")]
    public required string PlexKey { get; init; }

    [QueryParam, BindFrom("width")]
    public required int Width { get; init; }

    [QueryParam, BindFrom("height")]
    public required int Height { get; init; }

    [QueryParam, BindFrom("metaDataKey")]
    public required int MetaDataKey { get; init; }
}

public class GetPlexMediaThumbnailImageEndpointRequestValidator : Validator<GetPlexMediaThumbnailImageEndpointRequest>
{
    public GetPlexMediaThumbnailImageEndpointRequestValidator()
    {
        RuleFor(x => x.PlexServerId).GreaterThan(0);
        RuleFor(x => x.PlexKey).NotEmpty();
        RuleFor(x => x.Width).GreaterThan(0);
        RuleFor(x => x.Height).GreaterThan(0);
        RuleFor(x => x.MetaDataKey).GreaterThan(0);
    }
}

public class GetPlexMediaThumbnailImageEndpoint : BaseEndpoint<GetPlexMediaThumbnailImageEndpointRequest, byte[]>
{
    private readonly ILogger _log;
    private readonly ICommandExecutor _commandExecutor;

    public override string EndpointPath => ApiRoutes.PlexMediaController + "/thumbnail";

    public GetPlexMediaThumbnailImageEndpoint(ILogger log, ICommandExecutor commandExecutor)
    {
        _log = log.ForContext<GetPlexMediaThumbnailImageEndpoint>();
        _commandExecutor = commandExecutor;
    }

    public override void Configure()
    {
        Get(EndpointPath);
        ResponseCache(259200); // Cache for 3 days
        Summary(s =>
        {
            s.Summary = "Proxy Plex image";
            s.Description = "Proxies image bytes from Plex servers with CORS headers.";
            s.ExampleRequest = new GetPlexMediaThumbnailImageEndpointRequest
            {
                PlexServerId = 1,
                PlexKey = "1756014789",
                Width = 627,
                Height = 938,
                MetaDataKey = 57920,
            };
        });

        Description(x =>
        {
            x.Produces(StatusCodes.Status200OK, typeof(byte[]), MediaTypeNames.Image.Jpeg)
                .Produces(StatusCodes.Status404NotFound, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status502BadGateway, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO));
        });
    }

    public override async Task HandleAsync(GetPlexMediaThumbnailImageEndpointRequest req, CancellationToken ct)
    {
        _log.Here().DebugApiCall(HttpContext, req);

        // Per-response CORS headers
        HttpContext.Response.Headers.AccessControlAllowOrigin = "*";
        HttpContext.Response.Headers.AccessControlAllowMethods = "GET, OPTIONS";
        HttpContext.Response.Headers.AccessControlAllowHeaders = "*";

        var imageResult = await _commandExecutor.Send(
            new GetThumbnailImageCommand
            {
                PlexServerId = req.PlexServerId,
                PlexKey = req.PlexKey,
                MetaDataKey = req.MetaDataKey,
                Width = req.Width,
                Height = req.Height,
            },
            ct
        );

        if (imageResult.IsFailed)
        {
            _log.Here()
                .Warning("Failed to build Plex image URL: {Error}", imageResult.Errors.FirstOrDefault()?.Message);
            await SendFluentResult(imageResult.ToResult(), ct);
            return;
        }

        await Send.BytesAsync(imageResult.Value.Data, contentType: imageResult.Value.ContentType, cancellation: ct);
    }
}
