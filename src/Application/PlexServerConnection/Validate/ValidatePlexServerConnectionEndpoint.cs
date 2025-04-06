using Application.Contracts;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using PlexApi.Contracts;

namespace PlexRipper.Application;

public record ValidatePlexServerConnectionEndpointRequest
{
    public required string Url { get; init; }
}

public class ValidatePlexServerConnectionEndpointRequestValidator
    : Validator<ValidatePlexServerConnectionEndpointRequest>
{
    public ValidatePlexServerConnectionEndpointRequestValidator()
    {
        RuleFor(x => x.Url)
            .NotEmpty()
            .WithMessage("URL cannot be empty.")
            .Must(BeAValidUrl)
            .WithMessage("string is not a valid URL");
    }

    private bool BeAValidUrl(string url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
        && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
}

public class ValidatePlexServerConnectionEndpoint
    : BaseEndpoint<ValidatePlexServerConnectionEndpointRequest, ResultDTO<ServerIdentityDTO>>
{
    private readonly IPlexApiService _plexApiService;

    public override string EndpointPath => ApiRoutes.PlexServerConnectionController + "/validate";

    public ValidatePlexServerConnectionEndpoint(IPlexApiService plexApiService)
    {
        _plexApiService = plexApiService;
    }

    public override void Configure()
    {
        Post(EndpointPath);

        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<ServerIdentityDTO>))
                .Produces(StatusCodes.Status400BadRequest, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(ValidatePlexServerConnectionEndpointRequest req, CancellationToken ct)
    {
        var result = await _plexApiService.ValidatePlexConnection(req.Url);

        await SendFluentResult(result, ct);
    }
}
