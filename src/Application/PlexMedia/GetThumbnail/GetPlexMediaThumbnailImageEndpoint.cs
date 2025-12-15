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
    public required int PlexKey { get; init; }

    [QueryParam, BindFrom("metaDataKey")]
    public required int MetaDataKey { get; init; }

    [QueryParam, BindFrom("width")]
    public required int Width { get; init; }

    [QueryParam, BindFrom("height")]
    public required int Height { get; init; }
}

public class GetPlexMediaThumbnailImageEndpointRequestValidator : Validator<GetPlexMediaThumbnailImageEndpointRequest>
{
    public GetPlexMediaThumbnailImageEndpointRequestValidator()
    {
        RuleFor(x => x.PlexServerId).GreaterThan(0);
        RuleFor(x => x.PlexKey).GreaterThan(0);
        RuleFor(x => x.MetaDataKey).GreaterThan(0);
        RuleFor(x => x.Width).InclusiveBetween(1, 4096);
        RuleFor(x => x.Height).InclusiveBetween(1, 4096);
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
        Summary(s =>
        {
            s.Summary = "Proxy Plex image";
            s.Description = "Proxies image bytes from Plex servers with CORS headers.";
            s.ExampleRequest = new GetPlexMediaThumbnailImageEndpointRequest
            {
                PlexServerId = 1,
                PlexKey = 1756014789,
                MetaDataKey = 57920,
                Width = 200,
                Height = 400,
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
        _log.Here().VerboseApiCall(HttpContext, req);

#pragma warning disable ASP0015
        HttpContext.Response.Headers["Access-Control-Allow-Origin"] = "*";
        HttpContext.Response.Headers["Access-Control-Allow-Methods"] = "GET, OPTIONS";
        HttpContext.Response.Headers["Access-Control-Allow-Headers"] = "*";

        // Client cache per-URL (querystring), avoid server-side response cache collisions
        HttpContext.Response.Headers["Cache-Control"] = "public, max-age=259200, immutable"; // 3 days
#pragma warning restore ASP0015

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
