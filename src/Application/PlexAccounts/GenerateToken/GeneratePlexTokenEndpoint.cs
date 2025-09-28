using FastEndpoints;
using FluentValidation;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;
using Reaparr.PlexApi.Contracts;

namespace Reaparr.Application;

public record GeneratePlexTokenEndpointRequest
{
    /// <summary>
    /// NOTE: This constructor is needed to make the query param optional in the front-end typescript-api generation.
    /// </summary>
    public GeneratePlexTokenEndpointRequest(string verificationCode = "")
    {
        VerificationCode = verificationCode;
    }

    public required int PlexAccountId { get; init; }

    [QueryParam, BindFrom("verificationCode")]
    public string VerificationCode { get; init; }
}

public class GeneratePlexTokenEndpointRequestValidator : Validator<GeneratePlexTokenEndpointRequest>
{
    public GeneratePlexTokenEndpointRequestValidator()
    {
        RuleFor(x => x.PlexAccountId).GreaterThan(0);
    }
}

public class GeneratePlexTokenResponse
{
    public required bool IsUnAuthorized { get; init; }

    public required bool NeedsVerificationCode { get; init; }

    public required string PlexAuthToken { get; init; }
}

public class GeneratePlexTokenEndpoint : BaseEndpoint<GeneratePlexTokenEndpointRequest, GeneratePlexTokenResponse>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly ICommandExecutor _commandExecutor;

    public override string EndpointPath => ApiRoutes.PlexAccountController + "/generate-token/{PlexAccountId}";

    public GeneratePlexTokenEndpoint(ILogger log, IReaparrDbContext dbContext, ICommandExecutor commandExecutor)
    {
        _log = log.ForContext<GeneratePlexTokenEndpoint>();
        _dbContext = dbContext;
        _commandExecutor = commandExecutor;
    }

    public override void Configure()
    {
        Get(EndpointPath);

        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<GeneratePlexTokenResponse>))
                .Produces(StatusCodes.Status400BadRequest, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status401Unauthorized, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(GeneratePlexTokenEndpointRequest req, CancellationToken ct)
    {
        _log.Here().DebugApiCall(HttpContext, req);
        var plexAccount = await _dbContext.PlexAccounts.GetAsync(req.PlexAccountId, ct);
        if (plexAccount is null)
        {
            await SendFluentResult(ResultExtensions.EntityNotFound(nameof(PlexAccount), req.PlexAccountId), ct);
            return;
        }

        if (!string.IsNullOrEmpty(req.VerificationCode))
        {
            plexAccount.Is2Fa = true;
            plexAccount.VerificationCode = req.VerificationCode;
        }

        var validateResult = await _commandExecutor.Send(
            new PlexSignInCommand
            {
                Username = plexAccount.Username,
                Password = plexAccount.Password,
                VerificationCode = plexAccount.VerificationCode,
            },
            ct
        );

        if (validateResult.IsSuccess)
        {
            var response = new GeneratePlexTokenResponse
            {
                IsUnAuthorized = false,
                NeedsVerificationCode = false,
                PlexAuthToken = validateResult.Value.AuthenticationToken,
            };
            await SendFluentResult(Result.Ok(response), ct);
            return;
        }

        if (validateResult.HasPlexErrorEnterVerificationCode())
        {
            var response = new GeneratePlexTokenResponse
            {
                IsUnAuthorized = false,
                NeedsVerificationCode = true,
                PlexAuthToken = "",
            };
            await SendFluentResult(Result.Ok(response), ct);
            return;
        }

        if (validateResult.Has401UnauthorizedError())
        {
            var response = new GeneratePlexTokenResponse
            {
                IsUnAuthorized = true,
                NeedsVerificationCode = false,
                PlexAuthToken = "",
            };
            await SendFluentResult(Result.Ok(response), ct);
            return;
        }

        var result = Result.Ok();
        result.WithErrors(validateResult.Errors.Where(x => x.GetType() == typeof(PlexError)));
        await SendFluentResult(result, ct);
    }
}
