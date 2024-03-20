using Application.Contracts;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using PlexApi.Contracts;

namespace PlexRipper.Application;

public record Verify2FAPinEndpointRequest(string ClientId, int AuthPinId = 0);

public class Verify2FAPinEndpointRequestValidator : Validator<Verify2FAPinEndpointRequest>
{
    public Verify2FAPinEndpointRequestValidator()
    {
        RuleFor(x => x.ClientId).NotEmpty();
        RuleFor(x => x.AuthPinId).GreaterThanOrEqualTo(0);
    }
}

public class Verify2FAPinEndpoint : BaseEndpoint<Verify2FAPinEndpointRequest, AuthPin>
{
    private readonly IPlexApiService _plexApiService;

    public override string EndpointPath => ApiRoutes.PlexAccountController + "/authpin";

    public Verify2FAPinEndpoint(IPlexApiService plexApiService)
    {
        _plexApiService = plexApiService;
    }

    public override void Configure()
    {
        Get(EndpointPath);
        AllowAnonymous();
        Description(x => x
            .Produces(StatusCodes.Status200OK, typeof(ResultDTO<AuthPin>))
            .Produces(StatusCodes.Status400BadRequest, typeof(ResultDTO))
            .Produces(StatusCodes.Status500InternalServerError, typeof(ResultDTO)));
    }

    public override async Task HandleAsync(Verify2FAPinEndpointRequest req, CancellationToken ct)
    {
        Result<AuthPin> authPinResult;
        if (req.AuthPinId == 0)
            authPinResult = await _plexApiService.Get2FAPin(req.ClientId);
        else
            authPinResult = await _plexApiService.Check2FAPin(req.AuthPinId, req.ClientId);

        await SendFluentResult(authPinResult, x => x, ct);
    }
}