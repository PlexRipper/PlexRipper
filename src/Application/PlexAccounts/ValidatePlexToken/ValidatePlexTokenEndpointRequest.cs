using FastEndpoints;
using FluentValidation;
using Reaparr.Application.Contracts;
using Reaparr.PlexApi.Contracts;

namespace Reaparr.Application;

public record ValidatePlexTokenEndpointRequest
{
    public required string DisplayName { get; init; } = "UnknownDisplayName";
    public required string ManualAuthenticationToken { get; init; }
}

public class ValidatePlexTokenEndpointResponse
{
    public required bool IsUnAuthorized { get; set; }

    public required string ClientId { get; init; }

    public required string Username { get; init; }

    public required string Email { get; init; }

    public required string Title { get; init; }

    public required long PlexId { get; init; }

    public required string Uuid { get; init; }

    public required string CustomAuthenticationToken { get; init; }

    public required bool IsValidated { get; init; }

    public required DateTime? ValidatedAt { get; init; }

    public required bool Is2Fa { get; init; }
}

public class ValidatePlexTokenPlexAccountRequestValidator : Validator<ValidatePlexTokenEndpointRequest>
{
    public ValidatePlexTokenPlexAccountRequestValidator()
    {
        RuleFor(x => x.ManualAuthenticationToken).NotEmpty().MinimumLength(5);
    }
}

public class ValidatePlexTokenEndpoint
    : BaseEndpoint<ValidatePlexTokenEndpointRequest, ValidatePlexTokenEndpointResponse>
{
    private readonly ILogger _log;
    private readonly ICommandExecutor _commandExecutor;

    public override string EndpointPath => ApiRoutes.PlexAccountController + "/validate/token";

    public ValidatePlexTokenEndpoint(ILogger log, ICommandExecutor commandExecutor)
    {
        _log = log.ForContext<ValidatePlexTokenEndpoint>();
        _commandExecutor = commandExecutor;
    }

    public override void Configure()
    {
        Post(EndpointPath);

        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<ValidatePlexTokenEndpointResponse>))
                .Produces(StatusCodes.Status400BadRequest, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status401Unauthorized, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(ValidatePlexTokenEndpointRequest req, CancellationToken ct)
    {
        _log.Here().DebugApiCall(HttpContext, req);

        var validateResult = await _commandExecutor.Send(
            new ValidatePlexTokenCommand(req.ManualAuthenticationToken),
            ct
        );

        // If the PlexAPI rejects the token with 401 Unauthorized
        if (validateResult.HasPlex401UnauthorizedError())
        {
            _log.Here()
                .Warning(
                    "Failed to validate the PlexAccount Authentication Token for user {PlexAccountDisplayName} from the PlexApi",
                    req.DisplayName
                );

            var response = new ValidatePlexTokenEndpointResponse
            {
                IsUnAuthorized = true,
                ClientId = string.Empty,
                Username = string.Empty,
                Email = string.Empty,
                Title = string.Empty,
                PlexId = 0,
                Uuid = string.Empty,
                CustomAuthenticationToken = string.Empty,
                IsValidated = false,
                ValidatedAt = null,
                Is2Fa = false,
            };
            await SendFluentResult(Result.Ok(response), ct);
            return;
        }

        if (validateResult.IsSuccess)
        {
            _log.Here()
                .Information(
                    "Successfully validated the PlexAccount Authentication Token for user {PlexAccountDisplayName} from the PlexApi",
                    req.DisplayName
                );

            var response = new ValidatePlexTokenEndpointResponse
            {
                IsUnAuthorized = false,
                ClientId = validateResult.Value.ClientId,
                Username = validateResult.Value.Username,
                Email = validateResult.Value.Email,
                Title = validateResult.Value.Title,
                PlexId = validateResult.Value.PlexId,
                Uuid = validateResult.Value.Uuid,
                CustomAuthenticationToken = validateResult.Value.AuthenticationToken,
                IsValidated = validateResult.Value.IsValidated,
                ValidatedAt = validateResult.Value.ValidatedAt,
                Is2Fa = validateResult.Value.Is2Fa,
            };
            await SendFluentResult(Result.Ok(response), ct);
            return;
        }

        // Default: return all errors if none of the above conditions matched
        await SendFluentResult(validateResult, ct);
    }
}
