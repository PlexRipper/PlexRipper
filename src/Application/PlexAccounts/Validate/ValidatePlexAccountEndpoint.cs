using Application.Contracts;
using FastEndpoints;
using FluentValidation;
using Logging.Interface;
using Microsoft.AspNetCore.Http;
using PlexApi.Contracts;

namespace PlexRipper.Application;

/// <summary>
/// Validates the <see cref="PlexAccount"/> by calling the PlexAPI and confirming the PlexAccount can be used to login.
/// </summary>
public class ValidatePlexAccountEndpointRequest
{
    /// <summary>
    /// Validates the <see cref="PlexAccount"/> by calling the PlexAPI and confirming the PlexAccount can be used to login.
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

public class ValidatePlexAccountEndpointRequestValidator : Validator<ValidatePlexAccountEndpointRequest>
{
    public ValidatePlexAccountEndpointRequestValidator()
    {
        RuleFor(x => x.PlexAccount).NotNull();
        RuleFor(x => x.PlexAccount.Username).NotEmpty().When(m => !m.PlexAccount.IsAuthTokenMode);
        RuleFor(x => x.PlexAccount.Password).NotEmpty().When(m => !m.PlexAccount.IsAuthTokenMode);
        RuleFor(x => x.PlexAccount.AuthenticationToken).NotEmpty().When(m => m.PlexAccount.IsAuthTokenMode);
    }
}

public class ValidatePlexAccountEndpoint : BaseEndpoint<ValidatePlexAccountEndpointRequest, PlexAccountDTO>
{
    private readonly ILog _log;
    private readonly IPlexApiService _plexApiService;

    public override string EndpointPath => ApiRoutes.PlexAccountController + "/validate";

    public ValidatePlexAccountEndpoint(ILog log, IPlexApiService plexApiService)
    {
        _log = log;
        _plexApiService = plexApiService;
    }

    public override void Configure()
    {
        Post(EndpointPath);
        AllowAnonymous();
        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<PlexAccountDTO>))
                .Produces(StatusCodes.Status400BadRequest, typeof(ResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(ResultDTO))
        );
    }

    public override async Task HandleAsync(ValidatePlexAccountEndpointRequest req, CancellationToken ct)
    {
        var plexAccount = req.PlexAccount.ToModel();

        Result<PlexAccount> validateResult;
        if (plexAccount.IsAuthTokenMode)
            validateResult = await _plexApiService.ValidatePlexToken(plexAccount);
        else
            validateResult = await _plexApiService.PlexSignInAsync(plexAccount);

        if (validateResult.IsSuccess)
        {
            await SendFluentResult(SignInSuccess(plexAccount, validateResult), x => x.ToDTO(), ct);
            return;
        }

        await SendFluentResult(
            IsTwoFactorAuth(validateResult) ? SignInTwoFactorAuth(plexAccount) : validateResult,
            x => x.ToDTO(),
            ct
        );
    }

    private static Result<PlexAccount> SignInTwoFactorAuth(PlexAccount plexAccount)
    {
        plexAccount.Is2Fa = true;
        return Result.Ok(plexAccount);
    }

    private static bool IsTwoFactorAuth(Result<PlexAccount> plexSignInResult) =>
        plexSignInResult.HasPlexErrorEnterVerificationCode();

    private Result<PlexAccount> SignInSuccess(PlexAccount plexAccount, Result<PlexAccount> plexSignInResult)
    {
        _log.Debug(
            "The PlexAccount with displayName {PlexAccountDisplayName} has been validated",
            plexAccount.DisplayName
        );
        return plexSignInResult;
    }
}
