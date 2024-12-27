using Application.Contracts;
using Data.Contracts;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using PlexApi.Contracts;

namespace PlexRipper.Application;

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

public class GeneratePlexTokenEndpoint : BaseEndpoint<GeneratePlexTokenEndpointRequest, string>
{
    private readonly IPlexRipperDbContext _dbContext;
    private readonly IPlexApiService _plexApiService;

    public override string EndpointPath => ApiRoutes.PlexAccountController + "/generate-token/{PlexAccountId}";

    public GeneratePlexTokenEndpoint(IPlexRipperDbContext dbContext, IPlexApiService plexApiService)
    {
        _dbContext = dbContext;
        _plexApiService = plexApiService;
    }

    public override void Configure()
    {
        Get(EndpointPath);

        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<string>))
                .Produces(StatusCodes.Status400BadRequest, typeof(ResultDTO))
                .Produces(StatusCodes.Status401Unauthorized, typeof(ResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(ResultDTO))
        );
    }

    public override async Task HandleAsync(GeneratePlexTokenEndpointRequest req, CancellationToken ct)
    {
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

        var validateResult = await _plexApiService.PlexSignInAsync(plexAccount);

        if (validateResult.IsSuccess)
        {
            await SendFluentResult(validateResult, x => x.AuthenticationToken, ct);
            return;
        }

        if (validateResult.HasPlexErrorEnterVerificationCode())
        {
            await SendFluentResult(validateResult, ct);
            return;
        }

        await SendFluentResult(validateResult.ToResult(), ct);
    }
}
