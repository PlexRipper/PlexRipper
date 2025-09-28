using FastEndpoints;
using FluentValidation;
using Reaparr.Application.Contracts;
using Reaparr.PlexApi.Contracts;

namespace Reaparr.Application;

/// <summary>
/// Validates the <see cref="PlexAccount"/> by calling the PlexAPI and confirming the PlexAccount can be used to log in.
/// </summary>
public record ValidatePlexCredentialsEndpointRequest
{
    public required string DisplayName { get; init; } = "UnknownDisplayName";

    public required string Username { get; init; }

    public required string Password { get; init; }

    public required string VerificationCode { get; set; }
}

public class ValidatePlexCredentialsResponse
{
    public required bool IsUnAuthorized { get; set; }

    public required string ClientId { get; init; }

    public required string Username { get; init; }

    public required string Password { get; init; }

    public required string Email { get; init; }

    public required string Title { get; init; }

    public required long PlexId { get; init; }

    public required string Uuid { get; init; }

    public required string AuthenticationToken { get; init; }

    public required bool IsValidated { get; init; }

    public required DateTime? ValidatedAt { get; init; }

    public required bool Is2Fa { get; init; }
}

public class ValidatePlexCredentialsEndpointRequestValidator : Validator<ValidatePlexCredentialsEndpointRequest>
{
    public ValidatePlexCredentialsEndpointRequestValidator()
    {
        RuleFor(x => x.Username).NotEmpty().MinimumLength(5);

        RuleFor(x => x.Password).NotEmpty().MinimumLength(5);
    }
}

public class ValidatePlexCredentialsEndpoint
    : BaseEndpoint<ValidatePlexCredentialsEndpointRequest, ValidatePlexCredentialsResponse>
{
    private readonly ILogger _log;
    private readonly ICommandExecutor _commandExecutor;

    public override string EndpointPath => ApiRoutes.PlexAccountController + "/validate/credentials";

    public ValidatePlexCredentialsEndpoint(ILogger log, ICommandExecutor commandExecutor)
    {
        _log = log.ForContext<ValidatePlexCredentialsEndpoint>();
        _commandExecutor = commandExecutor;
    }

    public override void Configure()
    {
        Post(EndpointPath);

        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<ValidatePlexCredentialsResponse>))
                .Produces(StatusCodes.Status400BadRequest, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status401Unauthorized, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(ValidatePlexCredentialsEndpointRequest req, CancellationToken ct)
    {
        _log.Here().DebugApiCall(HttpContext, req);

        var signInResult = await _commandExecutor.Send(
            new PlexSignInCommand
            {
                Username = req.Username,
                Password = req.Password,
                VerificationCode = req.VerificationCode,
            },
            ct
        );

        var isUnAuthorized = false;
        if (signInResult.IsSuccess)
        {
            _log.Here().Debug("The PlexAccount with displayName {Name} has been validated", req.DisplayName);
        }

        // If the PlexAPI returns a 2fa error, we need to return a verification code to the client.
        if (signInResult.HasPlexErrorEnterVerificationCode())
        {
            isUnAuthorized = true;
            _log.Here()
                .Warning("The PlexAccount with displayName {Name} requires a verification code (2FA)", req.DisplayName);
        }

        // If the PlexAPI rejects the credentials
        if (signInResult.HasPlex401UnauthorizedError())
        {
            isUnAuthorized = true;
            _log.Here().Warning("Invalid Plex credentials provided for username {Username}", req.Username);
        }

        var response = Result.Ok(
            new ValidatePlexCredentialsResponse
            {
                IsUnAuthorized = isUnAuthorized,
                ClientId = signInResult.Value.ClientId,
                Username = signInResult.Value.Username,
                Password = signInResult.Value.Password,
                Email = signInResult.Value.Email,
                Title = signInResult.Value.Title,
                PlexId = signInResult.Value.PlexId,
                Uuid = signInResult.Value.Uuid,
                AuthenticationToken = signInResult.Value.AuthenticationToken,
                IsValidated = signInResult.Value.IsValidated,
                ValidatedAt = signInResult.Value.ValidatedAt,
                Is2Fa = signInResult.Value.Is2Fa,
            }
        );

        await SendFluentResult(response, ct);
    }
}
