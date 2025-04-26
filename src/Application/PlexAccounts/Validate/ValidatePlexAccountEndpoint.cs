using Application.Contracts;
using FastEndpoints;
using FluentValidation;
using Logging.Interface;
using Microsoft.AspNetCore.Http;
using PlexApi.Contracts;

namespace PlexRipper.Application;

/// <summary>
/// Validates the <see cref="PlexAccount"/> by calling the PlexAPI and confirming the PlexAccount can be used to log in.
/// </summary>
public class ValidatePlexAccountEndpointRequest
{
    /// <summary>
    /// Validates the <see cref="PlexAccount"/> by calling the PlexAPI and confirming the PlexAccount can be used to log in.
    /// </summary>
    /// <param name="plexAccount">The <see cref="PlexAccount"/> to validate.</param>
    public ValidatePlexAccountEndpointRequest(PlexAccountDTO plexAccount)
    {
        PlexAccount = plexAccount;
    }

    /// <summary>
    /// The <see cref="PlexAccount"/> to validate.
    /// </summary>
    [FromBody]
    public PlexAccountDTO PlexAccount { get; init; }
}

public class ValidatePlexAccountResponse
{
    public required bool IsUnAuthorized { get; set; }

    public required PlexAccountDTO PlexAccountDTO { get; set; }
}

public class ValidatePlexAccountEndpointRequestValidator : Validator<ValidatePlexAccountEndpointRequest>
{
    public ValidatePlexAccountEndpointRequestValidator()
    {
        RuleFor(x => x.PlexAccount).NotNull();
        RuleFor(x => x.PlexAccount.Username)
            .NotEmpty()
            .MinimumLength(5)
            .When(m => string.IsNullOrEmpty(m.PlexAccount.CustomAuthenticationToken));

        RuleFor(x => x.PlexAccount.Password)
            .NotEmpty()
            .MinimumLength(5)
            .When(m => string.IsNullOrEmpty(m.PlexAccount.CustomAuthenticationToken));
    }
}

public class ValidatePlexAccountEndpoint : BaseEndpoint<ValidatePlexAccountEndpointRequest, ValidatePlexAccountResponse>
{
    private readonly ILog _log;
    private readonly IPlexApiService _plexApiService;
    private readonly ICommandDispatch _commandDispatch;

    public override string EndpointPath => ApiRoutes.PlexAccountController + "/validate";

    public ValidatePlexAccountEndpoint(ILog log, IPlexApiService plexApiService, ICommandDispatch commandDispatch)
    {
        _log = log;
        _plexApiService = plexApiService;
        _commandDispatch = commandDispatch;
    }

    public override void Configure()
    {
        Post(EndpointPath);

        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<ValidatePlexAccountResponse>))
                .Produces(StatusCodes.Status400BadRequest, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status401Unauthorized, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(ValidatePlexAccountEndpointRequest req, CancellationToken ct)
    {
        var plexAccount = req.PlexAccount.ToModel();

        Result<PlexAccount> validateResult;
        if (plexAccount.IsAuthTokenMode)
        {
            validateResult = await _commandDispatch.ExecuteAsync(new ValidatePlexTokenCommand(plexAccount), ct);

            if (validateResult.IsSuccess)
            {
                _log.Information(
                    "Successfully validated the PlexAccount Authentication Token for user {PlexAccountDisplayName} from the PlexApi",
                    plexAccount.DisplayName
                );

                var response = Result.Ok(
                    new ValidatePlexAccountResponse
                    {
                        IsUnAuthorized = false,
                        PlexAccountDTO = validateResult.Value.ToDTO(),
                    }
                );
                await SendFluentResult(response, ct);
                return;
            }

            _log.Warning(
                "Failed to validate the PlexAccount Authentication Token for user {PlexAccountDisplayName} from the PlexApi",
                plexAccount.DisplayName
            );
        }
        else
        {
            validateResult = await _commandDispatch.ExecuteAsync(new PlexSignInCommand(plexAccount), ct);

            if (validateResult.IsSuccess)
            {
                _log.Debug(
                    "The PlexAccount with displayName {PlexAccountDisplayName} has been validated",
                    plexAccount.DisplayName
                );
                var response = Result.Ok(
                    new ValidatePlexAccountResponse
                    {
                        IsUnAuthorized = false,
                        PlexAccountDTO = validateResult.Value.ToDTO(),
                    }
                );
                await SendFluentResult(response, ct);
                return;
            }

            // If the PlexAPI returns a 2fa error, we need to return a verification code to the client.
            if (validateResult.HasPlexErrorEnterVerificationCode())
            {
                plexAccount.Is2Fa = true;

                var response = new ValidatePlexAccountResponse
                {
                    IsUnAuthorized = false,
                    PlexAccountDTO = plexAccount.ToDTO(),
                };
                await SendFluentResult(Result.Ok(response), ct);
                return;
            }
        }

        // We can't directly return a 401 Unauthorized status code, as it will cause the client to log out.
        if (validateResult.HasPlex401UnauthorizedError())
        {
            var response = new ValidatePlexAccountResponse
            {
                IsUnAuthorized = true,
                PlexAccountDTO = plexAccount.ToDTO(),
            };
            await SendFluentResult(Result.Ok(response), ct);
            return;
        }

        var result = Result.Ok();
        result.WithErrors(validateResult.Errors.FindAll(x => x.GetType() == typeof(PlexError)));
        await SendFluentResult(result, ct);
    }
}
