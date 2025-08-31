using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Reaparr.Application.Contracts;
using Reaparr.PlexApi.Contracts;

namespace Reaparr.Application;

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
    private readonly ICommandExecutor _commandExecutor;

    public override string EndpointPath => ApiRoutes.PlexServerConnectionController + "/validate";

    public ValidatePlexServerConnectionEndpoint(ICommandExecutor commandExecutor)
    {
        _commandExecutor = commandExecutor;
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
        var result = await _commandExecutor.Send(new ValidatePlexConnectionUrlCommand(req.Url), ct);
        await SendFluentResult(result, ct);
    }
}
